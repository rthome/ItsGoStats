using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Caching.Iteration
{
    public interface IEntityHandler
    {
        void OnAssist(int index, Assist entity);

        void OnDisconnect(int index, Disconnect entity);

        void OnGame(int index, Game entity);

        void OnKill(int index, Kill entity);

        void OnPurchase(int index, Purchase entity);

        void OnRound(int index, Round entity);

        void OnTeamSwitch(int index, TeamSwitch entity);
    }
}
