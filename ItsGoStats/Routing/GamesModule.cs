using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;

using Nancy;

namespace ItsGoStats.Routing
{
    public class GamesModule : NancyModule
    {
        public GamesModule()
            : base("/Games")
        {
            async Task<List<Game>> QueryGamesAsync(DateConstraint constraint)
            {
                var games = await DatabaseProvider.Connection.QueryAsync<Game>("select * from Game where Game.Time >= @Start and Game.Time < @End", new { constraint.Start, constraint.End });
                return games.AsList();
            }

            Get["/"] = _ =>
            {
                return Response.AsRedirect("/Games/Today");
            };

            Get["/{Date:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var games = await QueryGamesAsync(parameters.Date);
                return View["games.cshtml", games];
            };
        }
    }
}
