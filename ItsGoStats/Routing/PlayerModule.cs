using System.Threading.Tasks;

using ItsGoStats.Common;
using ItsGoStats.Models;

using Nancy;

namespace ItsGoStats.Routing
{
    public class PlayerModule : NancyModule
    {
        public const string BasePath = "/Player";

        public PlayerModule()
            : base(BasePath)
        {
            async Task<PlayerModel> CreateModelAsync(string steamId, DateConstraint constraint)
            {
                var player = await DatabaseProvider.GetPlayerAsync(steamId);
                if (player == null)
                    return null;

                var model = await PlayerModel.CreateAsync(player, constraint);
                return model;
            }

            Get["/{SteamId}"] = parameters =>
            {
                return Response.AsRedirect($"/Player/{parameters.SteamId}/AllTime");
            };

            Get["/{SteamId}/{Date:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var model = await CreateModelAsync(parameters.SteamId, parameters.Date);
                if (model == null)
                    return HttpStatusCode.NotFound;

                ViewBag.Title = model.Player.Name;

                return View[model];
            };

            Get["/{SteamId}/From/{StartDate:dateform}/To/{EndDate:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var constraint = DateConstraint.Merge(parameters.StartDate, parameters.EndDate);
                var model = await CreateModelAsync(parameters.SteamId, parameters.Date);
                if (model == null)
                    return HttpStatusCode.NotFound;

                ViewBag.Title = model.Player.Name;

                return View[model];
            };
        }
    }
}
