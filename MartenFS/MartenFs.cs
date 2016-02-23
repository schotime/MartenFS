using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace MartenFS
{
    public class MartenFS : IDisposable
    {
        private readonly NpgsqlConnection _npgsqlConnection;
        private readonly bool _shouldDisposeConnection;

        public char PathSeparator { get; set; } = '.';
        public IEncryption Encryption { get; set; } = new NoEncrytion();

        public NpgsqlConnection Connection => _npgsqlConnection;

        public MartenFS(NpgsqlConnection npgsqlConnection)
        {
            _npgsqlConnection = npgsqlConnection;
        }

        public MartenFS(string connectionString)
        {
            _npgsqlConnection = new NpgsqlConnection(connectionString);
            _shouldDisposeConnection = true;
        }

        private async Task OpenConnectionAsync()
        {
            if (_npgsqlConnection != null
                && _npgsqlConnection.State != ConnectionState.Broken 
                && _npgsqlConnection.State != ConnectionState.Closed)
                return;

            await _npgsqlConnection.OpenAsync();

            await CreateTableIfNotExists();
        }

        private async Task CreateTableIfNotExists()
        {
            var command = CommandBuilder.PrepareInsertTableCommand(this);
            await command.ExecuteNonQueryAsync();
        }
        
        public async Task<Guid> Save(string path, FileInfo fileinfo)
        {
            using (var fileStream = new FileStream(fileinfo.FullName, FileMode.Open))
            {
                return await Save(fileStream, MartenFile.FromFileInfo(path, fileinfo));
            }
        }

        public async Task<Guid> Save(byte[] bytes, MartenFile martinFile)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return await Save(memoryStream, martinFile);
            }
        }

        public async Task<Guid> Save(Stream streamIn, MartenFile martinFile)
        {
            await OpenConnectionAsync();

            var manager = new NpgsqlLargeObjectManager(_npgsqlConnection);

            using (var transaction = _npgsqlConnection.BeginTransaction())
            {
                uint oid = await manager.CreateAsync();

                int bytes = 0;
                Encryption.Encrypt(await manager.OpenReadWriteAsync(oid), stream =>
                {
                    bytes = Util.CopyToInternal(streamIn, stream, 81920);
                });

                martinFile.SetSize(bytes);
                martinFile.SetContentId(oid);

                using (var command = CommandBuilder.PrepareInsertCommand(this, martinFile))
                {
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }

            return martinFile.Id;
        }

        public void Dispose()
        {
            if (_shouldDisposeConnection)
                _npgsqlConnection.Dispose();
        }

        public async Task<MartenFile> GetFileDetail(Guid id)
        {
            await OpenConnectionAsync();

            using (var command = CommandBuilder.PrepareGetFileMetaDataCommand(this, id))
            {
                var reader = await command.ExecuteReaderAsync();
                return ReadMartenFile(reader).SingleOrDefault();
            }
        }

        public async Task<IEnumerable<MartenFile>> GetFileDetails(string pathQuery)
        {
            await OpenConnectionAsync();

            using (var command = CommandBuilder.PrepareGetFileMetaDatasCommand(this, Util.NormalizePath(pathQuery, PathSeparator, false)))
            {
                var reader = await command.ExecuteReaderAsync();
                return ReadMartenFile(reader);
            }
        }

        private IEnumerable<MartenFile> ReadMartenFile(DbDataReader reader)
        {
            try
            {
                while (reader.Read())
                {
                    var martenFile = MartenFile.FromData(reader.GetString(2), Util.DeNormalizePath(reader.GetString(1), PathSeparator));
                    martenFile.Id = reader.GetGuid(0);
                    martenFile.Size = reader.GetInt32(3);
                    martenFile.Modified = reader.GetDateTime(4);
                    martenFile.Created = reader.GetDateTime(5);
                    //martenFile.Meta = reader.GetString(6);
                    martenFile.ContentId = reader.GetInt32(7);

                    yield return martenFile;
                }
            }
            finally
            {
                reader.Dispose();
            }
        }

        public async Task<byte[]> GetFile(Guid id)
        {
            using (var memoryStream = new MemoryStream())
            {
                await GetFile(id, memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task GetFile(Guid id, Stream outStream)
        {
            await OpenConnectionAsync();

            var manager = new NpgsqlLargeObjectManager(_npgsqlConnection);

            using (var transaction = _npgsqlConnection.BeginTransaction())
            {
                using (var command = CommandBuilder.PrepareGetFileOidCommand(this, id))
                {
                    uint oid = Convert.ToUInt32(await command.ExecuteScalarAsync());
                    if (oid == 0)
                        return;

                    using (var stream = await manager.OpenReadAsync(oid))
                    {
                        Encryption.Decrypt(outStream, encStream => stream.CopyTo(encStream));
                    }
                }
                
                transaction.Commit();
            }
        }

        public async Task Delete(Guid id)
        {
            await OpenConnectionAsync();

            var manager = new NpgsqlLargeObjectManager(_npgsqlConnection);

            using (var transaction = _npgsqlConnection.BeginTransaction())
            {
                using (var command = CommandBuilder.PrepareGetFileOidCommand(this, id))
                {
                    uint oid = Convert.ToUInt32(await command.ExecuteScalarAsync());
                    manager.Unlink(oid);
                }

                using (var command = CommandBuilder.PrepareDeleteFileCommand(this, id))
                {
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
        }
    }
}
