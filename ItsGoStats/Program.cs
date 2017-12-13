using Dapper;
using Dapper.Contrib.Extensions;
using ItsGoStats.Caching;
using ItsGoStats.Caching.Entities;
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
            var connection = new SQLiteConnection("Data Source=test.db; Version=3; Foreign Keys=True; Page Size=16384");
            await connection.OpenAsync();
            await DatabaseSchema.CreateTables(connection);

            await connection.InsertAsync(new Player { Name = "asd", SteamId = "1:0:1:1231231", NameTime = DateTime.Now });

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => MainAsync(args));
    }
}
