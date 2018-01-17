using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Caching.Iteration
{
    public class EntityStream
    {
        readonly IReadOnlyList<IEntityWithTimestamp> entities;

        #region Static Query/Creation Methods

        static async Task<List<IEntityWithTimestamp>> QueryEntitiesForGamesAsync(IDbConnection connection, params int[] gameIds)
        {
            var games = await connection.QueryAsync<Game>("select * from Game where Id in @Ids order by Time", new { Ids = gameIds });
            var rounds = await connection.QueryAsync<Round>("select * from Round where GameId in @Ids order by Time", new { Ids = gameIds });

            var parameters = new { GameIds = gameIds, RoundIds = rounds.Select(r => r.Id).ToList() };
            var entitySets = new IEnumerable<IEntityWithTimestamp>[]
            {
                games,
                rounds,
                await connection.QueryAsync<Assist>("select * from Assist where Assist.RoundId in @RoundIds order by Assist.Time", parameters),
                await connection.QueryAsync<Disconnect>("select * from Disconnect where Disconnect.GameId in @GameIds order by Disconnect.Time", parameters),
                await connection.QueryAsync<Kill>("select * from Kill where Kill.RoundId in @RoundIds order by Kill.Time", parameters),
                await connection.QueryAsync<Purchase>("select * from Purchase where Purchase.RoundId in @RoundIds order by Purchase.Time", parameters),
                await connection.QueryAsync<TeamSwitch>("select * from TeamSwitch where TeamSwitch.GameId in @GameIds order by TeamSwitch.Time", parameters),
            };

            var results = entitySets.SelectMany(_ => _).ToList();
            results.Sort((l, r) => l.Time.CompareTo(r.Time));
            return results;
        }

        public static async Task<EntityStream> CreateAsync(IDbConnection connection, int gameId)
        {
            var entities = await QueryEntitiesForGamesAsync(connection, gameId);
            return new EntityStream(entities);
        }

        public static async Task<EntityStream> CreateAsync(IDbConnection connection, DateTime start, DateTime end)
        {
            var gameIds = await connection.QueryAsync<int>("select Id from Game where Game.Time >= @Start and Game.Time < @End", new { Start = start, End = end });
            var entities = await QueryEntitiesForGamesAsync(connection, gameIds.ToArray());
            return new EntityStream(entities);
        }

        #endregion

        public void Execute(params IEntityHandler[] handlers)
        {
            var handlerList = handlers.AsList();

            handlerList.ForEach(h => h.OnStart());
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                switch (entity)
                {
                    case Assist assist:
                        handlerList.ForEach(h => h.OnAssist(i, assist));
                        break;
                    case Disconnect disconnect:
                        handlerList.ForEach(h => h.OnDisconnect(i, disconnect));
                        break;
                    case Game game:
                        handlerList.ForEach(h => h.OnGame(i, game));
                        break;
                    case Kill kill:
                        handlerList.ForEach(h => h.OnKill(i, kill));
                        break;
                    case Purchase purchase:
                        handlerList.ForEach(h => h.OnPurchase(i, purchase));
                        break;
                    case Round round:
                        handlerList.ForEach(h => h.OnRound(i, round));
                        break;
                    case TeamSwitch teamSwitch:
                        handlerList.ForEach(h => h.OnTeamSwitch(i, teamSwitch));
                        break;
                }
            }
            handlerList.ForEach(h => h.OnEnd());
        }

        public async Task ExecuteAsync(params IEntityHandler[] handlers) => await Task.Run(() => Execute(handlers));

        public EntityStream(IReadOnlyList<IEntityWithTimestamp> entities)
        {
            this.entities = entities;
        }
    }
}
