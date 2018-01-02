using Nancy;
using Dapper;
using ItsGoStats.Caching.Entities;
using System.Threading.Tasks;
using ItsGoStats.Common;
using ItsGoStats.Models;

namespace ItsGoStats.Routing
{
    public class PlayerModule : NancyModule
    {
        public PlayerModule()
            : base("/Player")
        {
            async Task<Player> GetPlayerAsync(string steamId)
            {
                return await DatabaseProvider.Connection.QuerySingleOrDefaultAsync<Player>(
                    "select * from Player where Player.SteamId = @SteamId", new { SteamId = steamId });
            }

            async Task<PlayerModel> CreatePlayerModelAsync(string steamId, DateConstraint constraint)
            {
                var player = await GetPlayerAsync(steamId);
                var model = await PlayerModel.CreateAsync(player, constraint);
                return model;
            }

            Get["/{SteamId}"] = parameters =>
            {
                return Response.AsRedirect($"/Player/{parameters.SteamId}/AllTime");
            };

            Get["/{SteamId}/{Date:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var model = await CreatePlayerModelAsync(parameters.SteamId, parameters.Date);
                ViewBag.Title = model.Player.Name;

                return View[model];
            };

            Get["/{SteamId}/From/{StartDate:dateform}/To/{EndDate:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var constraint = DateConstraint.Merge(parameters.StartDate, parameters.EndDate);
                var model = await CreatePlayerModelAsync(parameters.SteamId, parameters.Date);
                ViewBag.Title = model.Player.Name;

                return View[model];
            };
        }
    }
}
