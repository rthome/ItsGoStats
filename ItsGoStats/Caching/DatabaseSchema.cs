using System.Data;
using System.Threading.Tasks;

using Dapper;

namespace ItsGoStats.Caching
{
    static class DatabaseSchema
    {
        const string Assist = @"
            CREATE TABLE 'Assists' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                    'Time' DATETime NOT NULL,
                                    'RoundId' INTEGER NOT NULL,
                                    'AssisterId' INTEGER NOT NULL,
                                    'AssisterTeam' INTEGER NOT NULL,
                                    'VictimId' INTEGER NOT NULL,
                                    'VictimTeam' INTEGER NOT NULL,
                                    FOREIGN KEY ('RoundId') REFERENCES 'Rounds' ('Id') ON DELETE CASCADE,
                                    FOREIGN KEY ('AssisterId') REFERENCES 'Players' ('Id'),
                                    FOREIGN KEY ('VictimId') REFERENCES 'Players' ('Id'))";

        const string Disconnect = @"
            CREATE TABLE 'Disconnects' ('Id' INTEGER NOT NULL PRIMARY KEY, 
                                        'Time' DATETime NOT NULL,
                                        'GameId' INTEGER NOT NULL,
                                        'PlayerId' INTEGER NOT NULL,
                                        'Team' INTEGER NOT NULL,
                                        'Reason' VARCHAR(255) NOT NULL,
                                        FOREIGN KEY ('GameId') REFERENCES 'Games' ('Id') ON DELETE CASCADE,
                                        FOREIGN KEY ('PlayerId') REFERENCES 'Players' ('Id'))";

        const string Game = @"
            CREATE TABLE 'Games' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                  'Time' DATETime NOT NULL,
                                  'Map' VARCHAR(255) NOT NULL,
                                  'MaxRounds' INTEGER NOT NULL,
                                  'Outcome' INTEGER)";

        const string Kill = @"
            CREATE TABLE 'Kills' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                  'Time' DATETime NOT NULL,
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
                                  FOREIGN KEY ('RoundId') REFERENCES 'Rounds' ('Id') ON DELETE CASCADE,
                                  FOREIGN KEY ('KillerId') REFERENCES 'Players' ('Id'),
                                  FOREIGN KEY ('VictimId') REFERENCES 'Players' ('Id'))";

        const string Player = @"
            CREATE TABLE 'Players' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                    'SteamId' VARCHAR(32) NOT NULL,
                                    'NameTime' DATETime NOT NULL,
                                    'Name' VARCHAR(255) NOT NULL)";

        const string Purchase = @"
            CREATE TABLE 'Purchases' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                      'Time' DATETime NOT NULL,
                                      'RoundId' INTEGER NOT NULL,
                                      'PlayerId' INTEGER NOT NULL,
                                      'Team' INTEGER NOT NULL,
                                      'Item' VARCHAR(255) NOT NULL,
                                      FOREIGN KEY ('RoundId') REFERENCES 'Rounds' ('Id') ON DELETE CASCADE,
                                      FOREIGN KEY ('PlayerId') REFERENCES 'Players' ('Id'))";

        const string Round = @"
            CREATE TABLE 'Rounds' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                   'Time' DATETime NOT NULL,
                                   'GameId' INTEGER NOT NULL,
                                   'Winner' INTEGER NOT NULL,
                                   'SfuiNotice' TEXT NOT NULL,
                                   'TerroristScore' INTEGER NOT NULL,
                                   'CounterTerroristScore' INTEGER NOT NULL,
                                   FOREIGN KEY ('GameId') REFERENCES 'Games' ('Id') ON DELETE CASCADE)";

        const string TeamSwitch = @"
            CREATE TABLE 'TeamSwitches' ('Id' INTEGER NOT NULL PRIMARY KEY,
                                         'Time' DATETime NOT NULL,
                                         'GameId' INTEGER NOT NULL,
                                         'PlayerId' INTEGER NOT NULL,
                                         'PreviousTeam' INTEGER NOT NULL,
                                         'CurrentTeam' INTEGER NOT NULL,
                                         FOREIGN KEY ('GameId') REFERENCES 'Games' ('Id') ON DELETE CASCADE,
                                         FOREIGN KEY ('PlayerId') REFERENCES 'Players' ('Id'))";

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
            @"CREATE INDEX 'Assist_Time' ON 'Assists' ('Time')",
            @"CREATE INDEX 'Assist_RoundId' ON 'Assists' ('RoundId')",
            @"CREATE INDEX 'Assist_AssisterId' ON 'Assists' ('AssisterId')",
            @"CREATE INDEX 'Assist_VictimId' ON 'Assists' ('VictimId')",

            @"CREATE INDEX 'Disconnect_Time' ON 'Disconnects' ('Time')",
            @"CREATE INDEX 'Disconnect_GameId' ON 'Disconnects' ('GameId')",
            @"CREATE INDEX 'Disconnect_PlayerId' ON 'Disconnects' ('PlayerId')",

            @"CREATE INDEX 'Game_Time' ON 'Games' ('Time')",

            @"CREATE INDEX 'Kill_Time' ON 'Kills' ('Time')",
            @"CREATE INDEX 'Kill_RoundId' ON 'Kills' ('RoundId')",
            @"CREATE INDEX 'Kill_KillerId' ON 'Kills' ('KillerId')",
            @"CREATE INDEX 'Kill_VictimId' ON 'Kills' ('VictimId')",

            @"CREATE UNIQUE INDEX 'Player_SteamId' ON 'Players' ('SteamId')",
            @"CREATE INDEX 'Player_NameTime' ON 'Players' ('NameTime')",
            @"CREATE INDEX 'Player_SteamId_NameTime' ON 'Players' ('SteamId', 'NameTime')",

            @"CREATE INDEX 'Purchase_Time' ON 'Purchases' ('Time')",
            @"CREATE INDEX 'Purchase_RoundId' ON 'Purchases' ('RoundId')",
            @"CREATE INDEX 'Purchase_PlayerId' ON 'Purchases' ('PlayerId')",

            @"CREATE INDEX 'Round_Time' ON 'Rounds' ('Time')",
            @"CREATE INDEX 'Round_GameId' ON 'Rounds' ('GameId')",

            @"CREATE INDEX 'TeamSwitch_Time' ON 'TeamSwitches' ('Time')",
            @"CREATE INDEX 'TeamSwitch_GameId' ON 'TeamSwitches' ('GameId')",
            @"CREATE INDEX 'TeamSwitch_PlayerId' ON 'TeamSwitches' ('PlayerId')",
        };

        public static void CreateTables(IDbConnection connection)
        {
            using (var tr = connection.BeginTransaction())
            {
                foreach (var definition in TableDefinitions)
                    connection.Execute(definition, transaction: tr);
                foreach (var definition in IndexDefinitions)
                    connection.Execute(definition, transaction: tr);

                tr.Commit();
            }
        }
    }
}
