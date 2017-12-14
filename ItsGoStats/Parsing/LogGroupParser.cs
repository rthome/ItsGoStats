using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using ItsGoStats.Common;
using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Parsing
{
    class LogGroupParser
    {
        const string DatePrefix = @"^L (\d{2})\/(\d{2})\/(\d+) - (\d{2}):(\d{2}):(\d{2}): ";
        const string PlayerPattern = @"""(.+?)<\d+><(.+?)><(.*?)>""";
        const string PlayerWithoutTeamPattern = @"""(.+?)<\d+><(.+?)>""";

        static readonly Regex LogStartedRegex = new Regex(DatePrefix + @"Log file started \(file "".+?""\) \(game "".+?""\) \(version ""(\d+)""\)");
        static readonly Regex MatchStartRegex = new Regex(DatePrefix + @"World triggered ""Match_Start"" on ""(.+?)""");
        static readonly Regex KillRegex = new Regex(DatePrefix + PlayerPattern + @" \[(-?\d+) (-?\d+) (-?\d+)\] killed " + PlayerPattern + @" \[(-?\d+) (-?\d+) (-?\d+)\] with ""(.+?)""(?: \((.+?)\))?");
        static readonly Regex AssistRegex = new Regex(DatePrefix + PlayerPattern + @" assisted killing " + PlayerPattern);
        static readonly Regex CVarRegex = new Regex(DatePrefix + @"server_cvar: ""([^""]+)"" ""([^""]+)""");
        static readonly Regex EndOfRoundRegex = new Regex(DatePrefix + @"Team ""(TERRORIST|CT)"" triggered ""(SFUI_Notice_All_Hostages_Rescued|SFUI_Notice_Bomb_Defused|SFUI_Notice_CTs_Win|SFUI_Notice_Hostages_Not_Rescued|SFUI_Notice_Target_Bombed|SFUI_Notice_Target_Saved|SFUI_Notice_Terrorists_Win)"" \(CT ""(\d+)""\) \(T ""(\d+)""\)");
        static readonly Regex TeamSwitchRegex = new Regex(DatePrefix + PlayerWithoutTeamPattern + @" switched from team <(.+?)> to <(.+?)>");
        static readonly Regex DisconnectRegex = new Regex(DatePrefix + PlayerPattern + @" disconnected \(reason ""(.+?)""\)");
        static readonly Regex PurchaseRegex = new Regex(DatePrefix + PlayerPattern + @" purchased ""(.+?)""");

        static readonly Dictionary<Regex, Func<RegexReader, LogEventBase>> Readers = new Dictionary<Regex, Func<RegexReader, LogEventBase>>
        {
            { PurchaseRegex, ReadPurchase },
            { KillRegex, ReadKill },
            { CVarRegex, ReadCVar },
            { TeamSwitchRegex, ReadTeamSwitch },
            { AssistRegex, ReadAssist },
            { DisconnectRegex, ReadDisconnect },
            { EndOfRoundRegex, ReadEndOfRound },
            { MatchStartRegex, ReadGameStart },
            { LogStartedRegex, ReadServerVersion },
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

        public LogFileGroup FileGroup { get; }

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

            var assister = new PlayerData { SteamId = assisterSteamId, NameTime = time, Name = assisterName };
            var victim = new PlayerData { SteamId = victimSteamId, NameTime = time, Name = victimName };

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

            var player = new PlayerData { SteamId = steamId, NameTime = time, Name = name };

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

            var killer = new PlayerData { SteamId = killerSteamId, NameTime = time, Name = killerName };
            var victim = new PlayerData { SteamId = victimSteamId, NameTime = time, Name = victimName };

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

            var player = new PlayerData { SteamId = steamId, NameTime = time, Name = name };

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

            var player = new PlayerData { SteamId = steamId, NameTime = time, Name = name };

            return new TeamSwitchData
            {
                Time = time,
                Player = player,
                PreviousTeam = previousTeam,
                CurrentTeam = currentTeam,
            };
        }

        public IObservable<LogEventBase> Parse()
        {
            return FileGroup.ReadConcatenatedLines()
                .SelectMany(line => Readers.ToObservable()
                    .Select(kv => new { Match = kv.Key.Match(line), Selector = kv.Value })
                    .FirstOrDefaultAsync(r => r.Match.Success)
                    .Where(r => r != null)
                    .Select(r => r.Selector(new RegexReader(r.Match))));
        }

        public LogGroupParser(LogFileGroup fileGroup) => FileGroup = fileGroup ?? throw new ArgumentNullException(nameof(fileGroup));
    }
}
