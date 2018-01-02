using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                throw new NotImplementedException();
            }

            Get["/"] = _ =>
            {
                return Response.AsRedirect("/Games/Today");
            };

            Get["/{Date:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var games = await QueryGamesAsync(parameters.Date);

                ViewBag.Title = "Games";

                return View["games.cshtml", games];
            };
        }
    }
}
