using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching;
using ItsGoStats.Common;
using ItsGoStats.Parsing;

using Nancy.Hosting.Self;

using Nito.AsyncEx;

namespace ItsGoStats
{
    class Program
    {
        static async Task Startup(IDbConnection connection, string logPath)
        {
            var logFileGroups = LogGroup.FromDirectory(logPath).AsList();
            foreach (var group in logFileGroups)
            {
                var parser = new LogGroupParser(group);
                var inserter = new LogGroupInserter(parser);
                await inserter.InsertEventsAsync(connection);
            }

            var dumpFile = "dump.db";
            if (File.Exists(dumpFile))
                File.Delete(dumpFile);
            await DatabaseSchema.DumpDatabaseAsync(connection, dumpFile);
        }

        static async Task<int> AsyncMain(string[] args)
        {
            await DatabaseProvider.InitializeAsync();

            if (args.Length > 0)
                await Startup(DatabaseProvider.Connection, args[0]);

            var hostConfig = new HostConfiguration
            {
                
            };

            var listenUri = new Uri("http://localhost:5555");
            using (var host = new NancyHost(hostConfig, listenUri))
            {
                host.Start();

                Console.WriteLine($"Listening on {listenUri}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }

            return 0;
        }

        static int Main(string[] args) => AsyncContext.Run(() => AsyncMain(args));
    }
}
