using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching;
using ItsGoStats.Common;
using ItsGoStats.Parsing;

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

            var sw = Stopwatch.StartNew();

            var groups = LogGroup.FromDirectory(args[0]);
            var parsers = groups.Select(grp => new LogGroupParser(grp));
            var inserters = parsers.Select(prs => new LogGroupInserter(prs));
            foreach (var inserter in inserters)
                await inserter.InsertEventsAsync(dbConnection);

            sw.Stop();

            var time = sw.Elapsed;
            Console.WriteLine($"Time: {time}");
            Console.WriteLine("Dumping...");

            if (File.Exists("test.db"))
                File.Delete("test.db");
            await DatabaseSchema.DumpDatabaseAsync(dbConnection, "test.db");

            Console.ReadKey();

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => AsyncMain(args));
    }
}
