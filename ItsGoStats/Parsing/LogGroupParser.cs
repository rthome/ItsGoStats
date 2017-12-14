using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

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
            {PurchaseRegex, ReadPurchase },
            {KillRegex, ReadKill },
            {CVarRegex, ReadCVar },
            {TeamSwitchRegex, ReadTeamSwitch },
            {AssistRegex, ReadAssist },
            {DisconnectRegex, ReadDisconnect },
            {EndOfRoundRegex, ReadEndOfRound },
            {MatchStartRegex, ReadGameStart },
            {LogStartedRegex, ReadServerVersion },
        };

        public LogFileGroup FileGroup { get; }

        static AssistData ReadAssist(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static CVarData ReadCVar(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static DisconnectData ReadDisconnect(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static EndOfRoundData ReadEndOfRound(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static GameStartData ReadGameStart(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static KillData ReadKill(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static PurchaseData ReadPurchase(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static ServerVersionData ReadServerVersion(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        static TeamSwitchData ReadTeamSwitch(RegexReader reader)
        {
            throw new NotImplementedException();
        }

        public IObservable<LogEventBase> Parse()
        {
            return FileGroup.ReadConcatenatedLines()
                .SelectMany(line => Readers.ToObservable()
                    .Select(kv => new { Match = kv.Key.Match(line), Selector = kv.Value })
                    .FirstOrDefaultAsync(r => r.Match != null)
                    .Select(r => r.Selector(new RegexReader(r.Match))));
        }

        public LogGroupParser(LogFileGroup fileGroup) => FileGroup = fileGroup ?? throw new ArgumentNullException(nameof(fileGroup));
    }
}
