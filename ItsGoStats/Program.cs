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

            if (File.Exists("test.db"))
                File.Delete("test.db");
            var connection = new SQLiteConnection("Data Source=test.db; Version=3; Foreign Keys=True; Page Size=16384");
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
            var parseTasks = parsers.Select(prs => Task.Run(prs.ParseAsync)).ToArray();
            var logEventLists = await Task.WhenAll(parseTasks);

            sw.Stop();

            var eventCount = logEventLists.Sum(l => l.Count);
            var time = sw.Elapsed;
            Console.WriteLine($"Events: {eventCount}");
            Console.WriteLine($"Time: {time}");

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => AsyncMain(args));
    }
}
