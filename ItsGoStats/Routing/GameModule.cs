using System.Threading.Tasks;

using Dapper.Contrib.Extensions;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Models;

using Nancy;

namespace ItsGoStats.Routing
{
    public class GameModule : NancyModule
    {
        public const string BasePath = "/Game";

        public GameModule()
            : base(BasePath)
        {
            async Task<GameModel> CreateModelAsync(int gameId)
            {
                var game = await DatabaseProvider.Connection.GetAsync<Game>(gameId);
                if (game == null)
                    return null;

                return await GameModel.CreateAsync(game);
            }

            Get["/{GameId:int}", runAsync: true] = async (parameters, token) =>
            {
                var model = await CreateModelAsync(parameters.GameId);
                if (model == null)
                    return HttpStatusCode.NotFound;

                return View[model];
            };
        }
    }
}
