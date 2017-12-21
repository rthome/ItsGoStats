using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Dapper;

namespace ItsGoStats.Caching
{
    public static class DatabaseSchema
    {
        const string Assist = @"
            CREATE TABLE 'Assist' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                   'Time' DATETIME NOT NULL,
                                   'RoundId' INTEGER NOT NULL,
                                   'AssisterId' INTEGER NOT NULL,
                                   'AssisterTeam' INTEGER NOT NULL,
                                   'VictimId' INTEGER NOT NULL,
                                   'VictimTeam' INTEGER NOT NULL,
                                   FOREIGN KEY ('RoundId') REFERENCES 'Round' ('Id') ON DELETE CASCADE,
                                   FOREIGN KEY ('AssisterId') REFERENCES 'Player' ('Id'),
                                   FOREIGN KEY ('VictimId') REFERENCES 'Player' ('Id'))";

        const string Disconnect = @"
            CREATE TABLE 'Disconnect' ('Id' INTEGER NOT NULL PRIMARY KEY, 
                                       'Time' DATETIME NOT NULL,
                                       'GameId' INTEGER NOT NULL,
                                       'PlayerId' INTEGER NOT NULL,
                                       'Team' INTEGER,
                                       'Reason' VARCHAR(255) NOT NULL,
                                       FOREIGN KEY ('GameId') REFERENCES 'Game' ('Id') ON DELETE CASCADE,
                                       FOREIGN KEY ('PlayerId') REFERENCES 'Player' ('Id'))";

        const string Game = @"
            CREATE TABLE 'Game' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                 'Time' DATETIME NOT NULL,
                                 'Map' VARCHAR(255) NOT NULL,
                                 'MaxRounds' INTEGER NOT NULL,
                                 'ElapsedMinutes' INTEGER NOT NULL,
                                 'FinalCounterTerroristScore' INTEGER NOT NULL,
                                 'FinalTerroristScore' INTEGER NOT NULL,
                                 'Outcome' INTEGER NOT NULL)";

        const string Kill = @"
            CREATE TABLE 'Kill' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                 'Time' DATETIME NOT NULL,
                                 'RoundId' INTEGER NOT NULL,
                                 'KillerId' INTEGER NOT NULL,
                                 'KillerTeam' INTEGER NOT NULL,
                                 'KillerPosition' VARCHAR(128) NOT NULL,
                                 'VictimId' INTEGER NOT NULL,
                                 'VictimTeam' INTEGER NOT NULL,
                                 'VictimPosition' VARCHAR(128) NOT NULL,
                                 'Headshot' INTEGER NOT NULL,
                                 'Penetrated' INTEGER NOT NULL,
                                 'Weapon' VARCHAR(255) NOT NULL,
                                 FOREIGN KEY ('RoundId') REFERENCES 'Round' ('Id') ON DELETE CASCADE,
                                 FOREIGN KEY ('KillerId') REFERENCES 'Player' ('Id'),
                                 FOREIGN KEY ('VictimId') REFERENCES 'Player' ('Id'))";

        const string Player = @"
            CREATE TABLE 'Player' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                   'SteamId' VARCHAR(32) NOT NULL,
                                   'NameTime' DATETIME NOT NULL,
                                   'Name' VARCHAR(255) NOT NULL)";

        const string Purchase = @"
            CREATE TABLE 'Purchase' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                     'Time' DATETIME NOT NULL,
                                     'RoundId' INTEGER NOT NULL,
                                     'PlayerId' INTEGER NOT NULL,
                                     'Team' INTEGER NOT NULL,
                                     'Item' VARCHAR(255) NOT NULL,
                                     FOREIGN KEY ('RoundId') REFERENCES 'Round' ('Id') ON DELETE CASCADE,
                                     FOREIGN KEY ('PlayerId') REFERENCES 'Player' ('Id'))";

        const string Round = @"
            CREATE TABLE 'Round' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                  'Time' DATETIME NOT NULL,
                                  'GameId' INTEGER NOT NULL,
                                  'Winner' INTEGER NOT NULL,
                                  'SfuiNotice' TEXT NOT NULL,
                                  'TerroristScore' INTEGER NOT NULL,
                                  'CounterTerroristScore' INTEGER NOT NULL,
                                  FOREIGN KEY ('GameId') REFERENCES 'Game' ('Id') ON DELETE CASCADE)";

        const string TeamSwitch = @"
            CREATE TABLE 'TeamSwitch' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                       'Time' DATETIME NOT NULL,
                                       'GameId' INTEGER NOT NULL,
                                       'PlayerId' INTEGER NOT NULL,
                                       'PreviousTeam' INTEGER NOT NULL,
                                       'CurrentTeam' INTEGER NOT NULL,
                                       FOREIGN KEY ('GameId') REFERENCES 'Game' ('Id') ON DELETE CASCADE,
                                       FOREIGN KEY ('PlayerId') REFERENCES 'Player' ('Id'))";

        public static readonly string[] TableDefinitions = new[]
        {
            Assist,
            Disconnect,
            Game,
            Kill,
            Player,
            Purchase,
            Round,
            TeamSwitch,
        };

        public static readonly string[] IndexDefinitions = new[]
        {
            @"CREATE INDEX 'Assist_Time' ON 'Assist' ('Time')",
            @"CREATE INDEX 'Assist_RoundId' ON 'Assist' ('RoundId')",
            @"CREATE INDEX 'Assist_AssisterId' ON 'Assist' ('AssisterId')",
            @"CREATE INDEX 'Assist_VictimId' ON 'Assist' ('VictimId')",

            @"CREATE INDEX 'Disconnect_Time' ON 'Disconnect' ('Time')",
            @"CREATE INDEX 'Disconnect_GameId' ON 'Disconnect' ('GameId')",
            @"CREATE INDEX 'Disconnect_PlayerId' ON 'Disconnect' ('PlayerId')",

            @"CREATE INDEX 'Game_Time' ON 'Game' ('Time')",
            @"CREATE INDEX 'Game_Outcome' ON 'Game' ('Outcome')",

            @"CREATE INDEX 'Kill_Time' ON 'Kill' ('Time')",
            @"CREATE INDEX 'Kill_RoundId' ON 'Kill' ('RoundId')",
            @"CREATE INDEX 'Kill_KillerId' ON 'Kill' ('KillerId')",
            @"CREATE INDEX 'Kill_VictimId' ON 'Kill' ('VictimId')",

            @"CREATE UNIQUE INDEX 'Player_SteamId' ON 'Player' ('SteamId')",
            @"CREATE INDEX 'Player_NameTime' ON 'Player' ('NameTime')",
            @"CREATE INDEX 'Player_SteamId_NameTime' ON 'Player' ('SteamId', 'NameTime')",

            @"CREATE INDEX 'Purchase_Time' ON 'Purchase' ('Time')",
            @"CREATE INDEX 'Purchase_RoundId' ON 'Purchase' ('RoundId')",
            @"CREATE INDEX 'Purchase_PlayerId' ON 'Purchase' ('PlayerId')",

            @"CREATE INDEX 'Round_Time' ON 'Round' ('Time')",
            @"CREATE INDEX 'Round_GameId' ON 'Round' ('GameId')",

            @"CREATE INDEX 'TeamSwitch_Time' ON 'TeamSwitch' ('Time')",
            @"CREATE INDEX 'TeamSwitch_GameId' ON 'TeamSwitch' ('GameId')",
            @"CREATE INDEX 'TeamSwitch_PlayerId' ON 'TeamSwitch' ('PlayerId')",
        };

        public static async Task CreateTablesAsync(IDbConnection connection)
        {
            using (var tr = connection.BeginTransaction())
            {
                foreach (var definition in TableDefinitions)
                    await connection.ExecuteAsync(definition, transaction: tr);
                foreach (var definition in IndexDefinitions)
                    await connection.ExecuteAsync(definition, transaction: tr);

                tr.Commit();
            }
        }

        public static async Task DumpDatabaseAsync(IDbConnection connection, string filename)
        {
            var userTables = (await connection.QueryAsync<string>(@"select name from sqlite_master where type='table' and name not like 'sqlite_%';")).AsList();

            using (var dump_connection = new SQLiteConnection(string.Format("Data Source={0}; Version=3; Page Size=16384", filename)))
            {
                await dump_connection.OpenAsync();
                await CreateTablesAsync(dump_connection);
            }

            var attachCommand = string.Format("attach '{0}' as 'ondisk';", filename);
            await connection.ExecuteAsync(attachCommand);
            await connection.ExecuteAsync("PRAGMA ondisk.foreign_keys=False");
            using (var tr = connection.BeginTransaction())
            {
                foreach (var table in userTables)
                {
                    var insertCommand = string.Format("insert into ondisk.{0} select * from main.{0};", table);
                    await connection.ExecuteAsync(insertCommand, transaction: tr);
                }

                tr.Commit();
            }
            await connection.ExecuteAsync("detach 'ondisk'");
        }
    }
}
