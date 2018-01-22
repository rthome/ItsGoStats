using System.Threading;
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

            async Task<dynamic> ShowPlayerAsync(string steamId, DateConstraint dateConstraint, CancellationToken token)
            {
                var model = await CreateModelAsync(steamId, dateConstraint);
                if (model == null)
                    return HttpStatusCode.NotFound;

                ViewBag.Date = dateConstraint;
                return View[model];
            }

            Get["/{SteamId}"] = parameters => Response.AsRedirect($"/Player/{parameters.SteamId}/Today");

            Get["/{SteamId}/{Date:dateform}", runAsync: true] = async (parameters, token) => await ShowPlayerAsync(parameters.SteamId, (DateConstraint)parameters.Date, token);

            Get["/{SteamId}/From/{StartDate}/To/{EndDate}", runAsync: true] = async (parameters, token) =>
            {
                if (DateConstraint.TryParse(parameters.StartDate, parameters.EndDate, out DateConstraint dateConstraint))
                    return await ShowPlayerAsync(parameters.SteamId, dateConstraint, token);
                else
                    return HttpStatusCode.NotFound;
            };
        }
    }
}
