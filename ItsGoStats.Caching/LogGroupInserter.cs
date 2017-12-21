using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Caching.States;
using ItsGoStats.Parsing;
using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Caching
{
    public class LogGroupInserter
    {
        static readonly Dictionary<Type, MethodInfo> handlerCache = new Dictionary<Type, MethodInfo>();

        readonly Dictionary<string, Player> playerCache = new Dictionary<string, Player>();
        readonly LogGroupParser parser;

        readonly ServerState serverState = new ServerState();

        IEnumerable<IReadOnlyList<LogEventBase>> ReadGames(IReadOnlyList<LogEventBase> events)
        {
            List<LogEventBase> gameEvents = null;
            foreach (var data in events)
            {
                if (data is CVarData cvar)
                {
                    serverState.CVars[cvar.Name] = cvar.Value;
                }
                else if (data is GameStartData gameStart)
                {
                    gameEvents = new List<LogEventBase> { data };
                }
                else if (data is GameOverData gameOver)
                {
                    gameEvents.Add(data);
                    yield return gameEvents;
                    gameEvents = null;
                }
                else
                {
                    gameEvents?.Add(data);
                }
            }
        }

        public async Task InsertEventsAsync(IDbConnection connection)
        {
            var events = await parser.ParseAsync();
            var games = ReadGames(events).AsList();

            foreach (var eventGroup in games)
            {
                var inserter = new GameInserter(serverState);
                await inserter.CreateGameAsync(connection, eventGroup);
            }
        }

        public LogGroupInserter(LogGroupParser parser) => this.parser = parser;
    }
}
