using System;
using System.Data.SQLite;
using System.IO;
using System.Reactive.Linq;

using Dapper;

using ItsGoStats.Caching;
using ItsGoStats.Common;
using ItsGoStats.Parsing;

namespace ItsGoStats
{
    class Program
    {
        static int Main(string[] args)
        {
            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            if (File.Exists("test.db"))
                File.Delete("test.db");
            var connection = new SQLiteConnection("Data Source=test.db; Version=3; Foreign Keys=True; Page Size=16384");
            connection.Open();

            DatabaseSchema.CreateTables(connection);

            if (args.Length > 0)
            {
                var logDir = args[0];
                var groups = LogFileGroup.FromDirectory(logDir);
                var events = groups
                    .Select(grp => new LogGroupParser(grp))
                    .SelectMany(parser => parser.Parse())
                    .ForEachAsync(Console.WriteLine);
            }

            return 0;
        }
    }
}
