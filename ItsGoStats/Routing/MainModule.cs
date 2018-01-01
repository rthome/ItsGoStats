using Dapper.Contrib.Extensions;
using Dapper;
using Nancy;
using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Routing
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/Highlights"] = _ =>
            {
                ViewBag.Title = "Highlights";

                return View["highlights.cshtml", null];
            };

            Get["/Games"] = _ =>
            {
                ViewBag.Title = "Games";

                return View["games.cshtml", null];
            };

            Get["/Players", runAsync: true] = async (_, token) =>
            {
                ViewBag.Title = "Players";

                var players = await DatabaseProvider.Connection.GetAllAsync<Player>();
                return View["players.cshtml", players.AsList()];
            };

            Get["/"] = _ =>
            {
                ViewBag.Title = "Index";

                return View["index.cshtml", null];
            };
        }
    }
}
