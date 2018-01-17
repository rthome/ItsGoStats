using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Caching.Iteration
{
    public abstract class EntityHandler : IEntityHandler
    {
        public virtual void OnStart() { }

        public virtual void OnEnd() { }

        public virtual void OnAssist(int index, Assist entity) { }

        public virtual void OnDisconnect(int index, Disconnect entity) { }

        public virtual void OnGame(int index, Game entity) { }

        public virtual void OnKill(int index, Kill entity) { }

        public virtual void OnPurchase(int index, Purchase entity) { }

        public virtual void OnRound(int index, Round entity) { }
        
        public virtual void OnTeamSwitch(int index, TeamSwitch entity) { }
    }
}
