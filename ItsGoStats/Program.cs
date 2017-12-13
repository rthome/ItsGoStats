using Dapper;
using Dapper.Contrib.Extensions;
using ItsGoStats.Caching;
using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using Nito.AsyncEx;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace ItsGoStats
{
    class Program
    {
        static async Task<int> MainAsync(string[] args)
        {
            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            var connection = new SQLiteConnection("Data Source=test.db; Version=3; Foreign Keys=True; Page Size=16384");
            await connection.OpenAsync();

            await DatabaseSchema.CreateTables(connection);

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => MainAsync(args));
    }
}
