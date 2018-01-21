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

        static async Task<ServerConfiguration> LoadConfigurationAsync(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
                return await ServerConfiguration.LoadAsync(args[0]);
            if (File.Exists("ItsGoStats.json"))
                return await ServerConfiguration.LoadAsync("ItsGoStats.json");

            // TODO: Log that we are running with the default config
            return new ServerConfiguration();
        }

        static async Task<int> AsyncMain(string[] args)
        {
            var configuration = await LoadConfigurationAsync(args);

            await DatabaseProvider.InitializeAsync();
            //await Startup(DatabaseProvider.Connection, configuration.LogDirectory);

            var hostConfig = new HostConfiguration
            {

            };

            var listenUri = new Uri(configuration.ListenUri);
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
