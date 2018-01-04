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
        readonly IEntityHandler handler;
        readonly IReadOnlyList<IEntityWithTimestamp> entities;

        #region Static Query/Creation Methods

        static async Task<List<IEntityWithTimestamp>> QueryEntitiesForGamesAsync(IDbConnection connection, params int[] gameIds)
        {
            const string Query = @"
                select * from Assist where Assist.GameId in @Ids order by Assist.Time
                select * from Disconnect where Disconnect.GameId in @Ids order by Disconnect.Time
                select * from Game where Game.GameId in @Ids order by Game.Time
                select * from Kill where Kill.GameId in @Ids order by Kill.Time
                select * from Purchase where Purchase.GameId in @Ids order by Purchase.Time
                select * from Round where Round.GameId in @Ids order by Round.Time
                select * from TeamSwitch where TeamSwitch.GameId in @Ids order by TeamSwitch.Time";

            using (var reader = await connection.QueryMultipleAsync(Query, new { Ids = gameIds }))
            {
                var entitySets = new IEnumerable<IEntityWithTimestamp>[]
                {
                    await reader.ReadAsync<Assist>(),
                    await reader.ReadAsync<Disconnect>(),
                    await reader.ReadAsync<Game>(),
                    await reader.ReadAsync<Kill>(),
                    await reader.ReadAsync<Purchase>(),
                    await reader.ReadAsync<Round>(),
                    await reader.ReadAsync<TeamSwitch>(),
                };

                var results = entitySets.SelectMany(_ => _).ToList();
                results.Sort((l, r) => l.Time.CompareTo(r.Time));
                return results;
            }
        }

        public static async Task<EntityStream> CreateAsync(IDbConnection connection, IEntityHandler handler, int gameId)
        {
            var entities = await QueryEntitiesForGamesAsync(connection, gameId);
            return new EntityStream(handler, entities);
        }

        public static async Task<EntityStream> CreateAsync(IDbConnection connection, IEntityHandler handler, DateTime start, DateTime end)
        {
            var gameIds = await connection.QueryAsync<int>("select Id from Game where Game.Time >= @Start and Game.Time < @End", new { Start = start, End = end });
            var entities = await QueryEntitiesForGamesAsync(connection, gameIds.ToArray());
            return new EntityStream(handler, entities);
        }

        #endregion

        public void Execute()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                switch (entity)
                {
                    case Assist assist:
                        handler.OnAssist(i, assist);
                        break;
                    case Disconnect disconnect:
                        handler.OnDisconnect(i, disconnect);
                        break;
                    case Game game:
                        handler.OnGame(i, game);
                        break;
                    case Kill kill:
                        handler.OnKill(i, kill);
                        break;
                    case Purchase purchase:
                        handler.OnPurchase(i, purchase);
                        break;
                    case Round round:
                        handler.OnRound(i, round);
                        break;
                    case TeamSwitch teamSwitch:
                        handler.OnTeamSwitch(i, teamSwitch);
                        break;
                }
            }
        }

        public async Task ExecuteAsync() => await Task.Run(() => Execute());

        public EntityStream(IEntityHandler handler, IReadOnlyList<IEntityWithTimestamp> entities)
        {
            this.handler = handler;
            this.entities = entities;
        }
    }
}
