using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Models;

using Nancy;

namespace ItsGoStats.Routing
{
    public class GamesModule : NancyModule
    {
        public const string BasePath = "/Games";

        async Task<List<Game>> QueryGamesAsync(DateConstraint constraint)
        {
            var games = await DatabaseProvider.Connection.QueryAsync<Game>("select * from Game where Game.Time >= @Start and Game.Time < @End", new { constraint.Start, constraint.End });
            return games.AsList();
        }

        async Task<dynamic> ShowGamesAsync(DateConstraint dateConstraint, CancellationToken token)
        {
            var games = await QueryGamesAsync(dateConstraint);
            var models = new List<GameModel>();
            foreach (var game in games)
                models.Add(await GameModel.CreateAsync(game));

            ViewBag.Date = dateConstraint;
            return View["games.cshtml", models];
        }

        public GamesModule()
            : base(BasePath)
        {
            Get["/"] = _ => Response.AsRedirect("/Games/Today");

            Get["/{Date:dateform}", runAsync: true] = async (parameters, token) => await ShowGamesAsync((DateConstraint)parameters.Date, token);

            Get["/From/{StartDate}/To/{EndDate}", runAsync: true] = async (parameters, token) =>
            {
                if (DateConstraint.TryParse(parameters.StartDate, parameters.EndDate, out DateConstraint dateConstraint))
                    return await ShowGamesAsync(dateConstraint, token);
                else
                    return HttpStatusCode.NotFound;
            };
        }
    }
}
