using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching;

namespace ItsGoStats
{
    static class DatabaseProvider
    {
        public static bool IsInitialized { get; private set; }

        public static IDbConnection Connection { get; private set; }

        public static async Task InitializeAsync()
        {
            if (IsInitialized)
                return;

            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            var conn = new SQLiteConnection("Data Source=:memory:; Version=3; Foreign Keys=True; Page Size=16384");
            await conn.OpenAsync();
            await DatabaseSchema.CreateTablesAsync(conn);

            Connection = conn;
            IsInitialized = true;
        }
    }
}
