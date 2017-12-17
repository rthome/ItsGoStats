using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ItsGoStats.Common;
using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Parsing
{
    class LogGroupParser
    {
        const string DatePrefix = @"^L (\d{2})\/(\d{2})\/(\d{4}) - (\d{2}):(\d{2}):(\d{2}): ";
        const string PlayerPattern = @"""(.+?)<\d+><(.+?)><(.*?)>""";
        const string PlayerWithoutTeamPattern = @"""(.+?)<\d+><(.+?)>""";

        static readonly Regex LogStartedRegex = new Regex(DatePrefix + @"Log file started \(file "".+?""\) \(game "".+?""\) \(version ""(\d+)""\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex MatchStartRegex = new Regex(DatePrefix + @"World triggered ""Match_Start"" on ""(.+?)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex KillRegex = new Regex(DatePrefix + PlayerPattern + @" \[(-?\d+) (-?\d+) (-?\d+)\] killed " + PlayerPattern + @" \[(-?\d+) (-?\d+) (-?\d+)\] with ""([^""]+)""(?: \(([^\)]+)\))?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex AssistRegex = new Regex(DatePrefix + PlayerPattern + @" assisted killing " + PlayerPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex CVarRegex = new Regex(DatePrefix + @"server_cvar: ""([^""]+)"" ""([^""]+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex EndOfRoundRegex = new Regex(DatePrefix + @"Team ""(TERRORIST|CT)"" triggered ""(SFUI_Notice_All_Hostages_Rescued|SFUI_Notice_Bomb_Defused|SFUI_Notice_CTs_Win|SFUI_Notice_Hostages_Not_Rescued|SFUI_Notice_Target_Bombed|SFUI_Notice_Target_Saved|SFUI_Notice_Terrorists_Win)"" \(CT ""(\d+)""\) \(T ""(\d+)""\)", RegexOptions.Compiled);
        static readonly Regex TeamSwitchRegex = new Regex(DatePrefix + PlayerWithoutTeamPattern + @" switched from team <(.+?)> to <(.+?)>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex DisconnectRegex = new Regex(DatePrefix + PlayerPattern + @" disconnected \(reason ""([^""]+)""\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        static readonly Regex PurchaseRegex = new Regex(DatePrefix + PlayerPattern + @" purchased ""([^""]+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        static readonly List<(string, Regex, Func<RegexReader, LogEventBase>)> Readers = new List<(string, Regex, Func<RegexReader, LogEventBase>)>
        {
            ("purchased", PurchaseRegex, ReadPurchase),
            ("killed", KillRegex, ReadKill),
            ("server_cvar", CVarRegex, ReadCVar),
            ("switched", TeamSwitchRegex, ReadTeamSwitch),
            ("assisted", AssistRegex, ReadAssist),
            ("disconnected", DisconnectRegex, ReadDisconnect),
            ("triggered", EndOfRoundRegex, ReadEndOfRound),
            ("Match_Start", MatchStartRegex, ReadGameStart),
            ("Log file", LogStartedRegex, ReadServerVersion),
        };

        static readonly Dictionary<string, Team> WinningTeamMapping = new Dictionary<string, Team>
        {
            { "SFUI_Notice_All_Hostages_Rescued", Team.CounterTerrorists },
            { "SFUI_Notice_Bomb_Defused", Team.CounterTerrorists },
            { "SFUI_Notice_CTs_Win", Team.CounterTerrorists },
            { "SFUI_Notice_Hostages_Not_Rescued", Team.Terrorists },
            { "SFUI_Notice_Target_Bombed", Team.Terrorists },
            { "SFUI_Notice_Target_Saved", Team.CounterTerrorists },
            { "SFUI_Notice_Terrorists_Win", Team.Terrorists },
        };

        const string BotId = "BOT";

        public LogGroup FileGroup { get; }

        static PlayerData GetPlayer(string steamId, DateTime nameTime, string name)
        {
            if (steamId == BotId)
                return new PlayerData { SteamId = BotId, NameTime = nameTime, Name = "Bot" };
            return new PlayerData { SteamId = steamId, NameTime = nameTime, Name = name };
        }

        static Team MapSfuiNotice(string value)
        {
            if (WinningTeamMapping.TryGetValue(value, out var winner))
                return winner;
            else
                throw new ArgumentException($"Unknown SfuiNotice text '{value}'", nameof(value));
        }

        static AssistData ReadAssist(RegexReader reader)
        {
            var time = reader.Date();
            var assisterName = reader.String();
            var assisterSteamId = reader.String();
            var assisterTeam = reader.Team().Value;
            var victimName = reader.String();
            var victimSteamId = reader.String();
            var victimTeam = reader.Team().Value;

            var assister = GetPlayer(assisterSteamId, time, assisterName);
            var victim = GetPlayer(victimSteamId, time, victimName);

            return new AssistData
            {
                Time = time,
                Assister = assister,
                AssisterTeam = assisterTeam,
                Victim = victim,
                VictimTeam = victimTeam,
            };
        }

        static CVarData ReadCVar(RegexReader reader)
        {
            var time = reader.Date();
            var name = reader.String();
            var value = reader.String();

            return new CVarData
            {
                Time = time,
                Name = name,
                Value = value,
            };
        }

        static DisconnectData ReadDisconnect(RegexReader reader)
        {
            var time = reader.Date();
            var name = reader.String();
            var steamId = reader.String();
            var team = reader.Team();
            var reason = reader.String();

            var player = GetPlayer(steamId, time, name);

            return new DisconnectData
            {
                Time = time,
                Player = player,
                Team = team,
                Reason = reason,
            };
        }

        static EndOfRoundData ReadEndOfRound(RegexReader reader)
        {
            var time = reader.Date();
            reader.Team();
            var sfuiNotice = reader.String();
            var counterTerroristScore = reader.Integer();
            var terroristScore = reader.Integer();

            return new EndOfRoundData
            {
                Time = time,
                Winner = MapSfuiNotice(sfuiNotice),
                SfuiNotice = sfuiNotice,
                CounterTerroristScore = counterTerroristScore,
                TerroristScore = terroristScore,
            };
        }

        static GameStartData ReadGameStart(RegexReader reader)
        {
            var time = reader.Date();
            var map = reader.String();

            return new GameStartData
            {
                Time = time,
                Map = map,
            };
        }

        static KillData ReadKill(RegexReader reader)
        {
            var time = reader.Date();
            var killerName = reader.String();
            var killerSteamId = reader.String();
            var killerTeam = reader.Team().Value;
            var killerPosition = reader.Vector();
            var victimName = reader.String();
            var victimSteamId = reader.String();
            var victimTeam = reader.Team().Value;
            var victimPosition = reader.Vector();
            var weapon = reader.String();
            var flags = reader.String();
            var headshot = (flags?.IndexOf("headshot")).GetValueOrDefault(-1) >= 0;
            var penetrated = (flags?.IndexOf("penetrated")).GetValueOrDefault(-1) >= 0;

            var killer = GetPlayer(killerSteamId, time, killerName);
            var victim = GetPlayer(victimSteamId, time, victimName);

            return new KillData
            {
                Time = time,
                Killer = killer,
                KillerTeam = killerTeam,
                KillerPosition = killerPosition,
                Victim = victim,
                VictimTeam = victimTeam,
                VictimPosition = victimPosition,
                Weapon = weapon,
                Headshot = headshot,
                Penetrated = penetrated,
            };
        }

        static PurchaseData ReadPurchase(RegexReader reader)
        {
            var time = reader.Date();
            var name = reader.String();
            var steamId = reader.String();
            var team = reader.Team().Value;
            var item = reader.String();

            var player = GetPlayer(steamId, time, name);

            return new PurchaseData
            {
                Time = time,
                Player = player,
                Team = team,
                Item = item,
            };
        }

        static ServerVersionData ReadServerVersion(RegexReader reader)
        {
            var time = reader.Date();
            var version = reader.Integer();

            return new ServerVersionData
            {
                Time = time,
                Version = version,
            };
        }

        static TeamSwitchData ReadTeamSwitch(RegexReader reader)
        {
            var time = reader.Date();
            var name = reader.String();
            var steamId = reader.String();
            var previousTeam = reader.Team();
            var currentTeam = reader.Team();

            var player = GetPlayer(steamId, time, name);

            return new TeamSwitchData
            {
                Time = time,
                Player = player,
                PreviousTeam = previousTeam,
                CurrentTeam = currentTeam,
            };
        }

        public async Task<IList<LogEventBase>> ParseAsync()
        {
            var lines = await FileGroup.ReadConcatenatedLinesAsync();
            var events = new List<LogEventBase>();
            for (int i = 0; i < lines.Length; i++)
            {
                foreach ((var guard, var regex, var handler) in Readers)
                {
                    var line = lines[i];
                    if (line.Length < 26 || line.IndexOf(guard, 25, StringComparison.Ordinal) < 0) // Start at position 25 to skip timestamp
                        continue;

                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var reader = new RegexReader(match);
                        events.Add(handler(reader));
                        break;
                    }
                }
            }
            return events;
        }

        public LogGroupParser(LogGroup fileGroup) => FileGroup = fileGroup ?? throw new ArgumentNullException(nameof(fileGroup));
    }
}
