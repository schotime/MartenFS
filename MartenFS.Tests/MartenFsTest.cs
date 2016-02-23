using System.Text;
using Npgsql;

namespace MartenFS.Tests
{
    public class MartenFsTest
    {
        public NpgsqlConnection Connection => new NpgsqlConnection("");
        public byte[] FakeBytes => Encoding.UTF8.GetBytes("Fake Content, please this is just some fake content");
        public string FakeName => "Fake Name";

        public MartenFsTest()
        {
            CleanDatabase();
        }

        private void CleanDatabase()
        {
            var conn = Connection;
            conn.Open();

            var command = conn.CreateCommand();
            command.CommandText = "SELECT lo_unlink(l.oid) FROM pg_largeobject_metadata l";
            command.ExecuteNonQuery();

            command.CommandText = @"DO 
$$
BEGIN

IF EXISTS (
    SELECT *
    FROM information_schema.tables
    WHERE
      table_schema = 'public' AND
      table_name = 'martenfs'
    ) THEN

        DELETE FROM martenfs;
    
    END IF;

END$$; ";
            command.ExecuteNonQuery();
        }
    }
}