using Dapper;
using Dapper.Contrib.Extensions;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;

using Nancy;

namespace ItsGoStats.Routing
{
    public class PlayersModule : NancyModule
    {
        public PlayersModule()
            : base("/Players")
        {
            Get["/", runAsync: true] = async (_, token) =>
            {
                var players = await DatabaseProvider.Connection.GetAllAsync<Player>();
                return View["players.cshtml", players.AsList()];
            };
        }
    }
}
