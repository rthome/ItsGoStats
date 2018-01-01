using Nancy;
using Dapper;
using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Routing
{
    public class PlayerModule : NancyModule
    {
        public PlayerModule()
            : base("/Player")
        {
            Get["/{SteamId}", runAsync: true] = async (parameters, token) =>
            {
                var player = await DatabaseProvider.Connection.QuerySingleOrDefaultAsync<Player>("select * from Player where Player.SteamId = @SteamId", new { parameters.SteamId });
                if (player == null)
                    return HttpStatusCode.NotFound;

                ViewBag.Title = player.Name;
                return View["player.cshtml", player];
            };
        }
    }
}
