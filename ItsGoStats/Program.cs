using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching;

using Nancy.Hosting.Self;

using Nito.AsyncEx;

namespace ItsGoStats
{
    class Program
    {
        static async Task<IDbConnection> PrepareDatabaseAsync()
        {
            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            var connection = new SQLiteConnection("Data Source=:memory:; Version=3; Foreign Keys=True; Page Size=16384");
            await connection.OpenAsync();

            await DatabaseSchema.CreateTablesAsync(connection);

            return connection;
        }

        static async Task<int> AsyncMain(string[] args)
        {
            var dbConnection = await PrepareDatabaseAsync();

            var listenUri = new Uri("http://localhost:5555");
            using (var host = new NancyHost(listenUri))
            {
                host.Start();

                Console.WriteLine($"Listening on {listenUri}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => AsyncMain(args));
    }
}
