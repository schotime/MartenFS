using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace MartenFS
{
    public class CommandBuilder
    {
        public static NpgsqlCommand PrepareInsertTableCommand(MartenFS martenFs)
        {
            var sql1 = @"
DO $$
BEGIN

IF NOT EXISTS (
    SELECT * 
    FROM information_schema.tables 
    WHERE 
      table_schema = 'public' AND 
      table_name = 'martenfs'
    ) THEN

    CREATE EXTENSION IF NOT EXISTS ltree;

    CREATE TABLE martenfs
    (
      id uuid NOT NULL primary key,
      path ltree NOT NULL,
      name text NOT NULL,
      size integer NOT NULL,
      modified timestamp without time zone,
      created timestamp without time zone,
      meta json,
      contentid integer NOT NULL
    );

    CREATE INDEX idx_martenfs__path ON martenfs USING GIST (path);
END IF;

END$$;

";
            var command = martenFs.Connection.CreateCommand();
            command.CommandText = sql1;
            return command;
        }

        public static NpgsqlCommand PrepareInsertCommand(MartenFS martenFs, MartenFile martenFile)
        {
            var command = martenFs.Connection.CreateCommand();
            var sql = @"
insert into martenfs (id, path, name, size, modified, created, meta, contentid)
values (@0, @1, @2, @3, @4, @5, @6, @7)
";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@0", martenFile.Id);
            command.Parameters.AddWithValue("@1", NpgsqlDbType.Unknown, Util.NormalizePath(martenFile.Path, martenFs.PathSeparator));
            command.Parameters.AddWithValue("@2", martenFile.Name);
            command.Parameters.AddWithValue("@3", martenFile.Size);
            command.Parameters.AddWithValue("@4", martenFile.Modified);
            command.Parameters.AddWithValue("@5", martenFile.Created);
            command.Parameters.AddWithValue("@6", martenFile.Meta == null ? DBNull.Value : (object)martenFile.Meta);
            command.Parameters.AddWithValue("@7", martenFile.ContentId);
            return command;
        }

        public static NpgsqlCommand PrepareGetFileOidCommand(MartenFS martenFs, Guid id)
        {
            var command = martenFs.Connection.CreateCommand();
            command.CommandText = @"select contentid from martenfs where id = @0";
            command.Parameters.AddWithValue("@0", id);
            return command;
        }

        public static NpgsqlCommand PrepareGetFileMetaDataCommand(MartenFS martenFs, Guid id)
        {
            var command = martenFs.Connection.CreateCommand();
            var sql = @"select id, path::text, name, size, modified, created, meta, contentid from martenfs where id = @0";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@0", id);
            return command;
        }

        public static NpgsqlCommand PrepareGetFileMetaDatasCommand(MartenFS martenFs, string query)
        {
            var command = martenFs.Connection.CreateCommand();
            var sql = @"select id, path::text, name, size, modified, created, meta, contentid from martenfs where path ~ @0";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@0", NpgsqlDbType.Unknown, query);
            return command;
        }

        public static NpgsqlCommand PrepareDeleteFileCommand(MartenFS martenFs, Guid id)
        {
            var command = martenFs.Connection.CreateCommand();
            command.CommandText = "delete from martenfs where id = @0";
            command.Parameters.AddWithValue("@0", id);
            return command;
        }
    }
}
