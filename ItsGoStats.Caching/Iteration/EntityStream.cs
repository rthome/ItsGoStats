using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Caching.Iteration
{
    public class EntityStream
    {
        readonly IEntityHandler handler;
        readonly IReadOnlyList<IEntity> entities;

        #region Static Query/Creation Methods

        public static Task<EntityStream> CreateAsync(IDbConnection connection, int gameId)
        {
            throw new NotImplementedException();
        }

        public static Task<EntityStream> CreateAsync(IDbConnection connection, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
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

        public EntityStream(IEntityHandler handler, IReadOnlyList<IEntity> entities)
        {
            this.handler = handler;
            this.entities = entities;
        }
    }
}
