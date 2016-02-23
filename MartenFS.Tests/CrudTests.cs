using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MartenFS.Tests
{
    public class CrudTests : MartenFsTest
    {
        [Fact]
        public async Task SaveAndGetTest()
        {
            using (var martenFs = new MartenFS(Connection) { Encryption = new AesEncryption("blah", "blah1112") })
            {
                var id = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName, "root"));

                var metaData = await martenFs.GetFileDetail(id);
                var data = await martenFs.GetFile(id);

                Assert.Equal("root", metaData.Path);
                Assert.Equal(FakeName, metaData.Name);
                Assert.Equal(FakeBytes, data);
            }
        }

        [Fact]
        public async Task AlternativePathSeparator()
        {
            using (var martenFs = new MartenFS(Connection) { PathSeparator = '/' })
            {
                var id = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName, "root/company1"));

                var metaData = await martenFs.GetFileDetail(id);
                var data = await martenFs.GetFile(id);

                Assert.Equal("root/company1", metaData.Path);
                Assert.Equal(FakeName, metaData.Name);
                Assert.Equal(FakeBytes, data);
            }
        }

        [Fact]
        public async Task GetFilesByPath()
        {
            using (var martenFs = new MartenFS(Connection) { PathSeparator = '/' })
            {
                var id1 = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName + "1", "root/company1"));
                var id2 = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName + "2", "root/company1"));
                var id3 = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName + "3", "root/company1"));

                var metaData = (await martenFs.GetFileDetails("root/company1")).OrderBy(x => x.Name).ToList();

                Assert.Equal(metaData.Count, 3);
                Assert.Equal(metaData[0].Name, FakeName + "1");
                Assert.Equal(metaData[1].Name, FakeName + "2");
                Assert.Equal(metaData[2].Name, FakeName + "3");
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            using (var martenFs = new MartenFS(Connection))
            {
                var id = await martenFs.Save(FakeBytes, MartenFile.FromData(FakeName, "root"));

                await martenFs.Delete(id);

                var metaData = await martenFs.GetFileDetail(id);
                var data = await martenFs.GetFile(id);

                Assert.Null(metaData);
                Assert.Equal(new byte[0], data);
            }
        }
    }
}
