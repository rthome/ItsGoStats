using Dapper.Contrib.Extensions;
using Dapper;
using Nancy;
using ItsGoStats.Caching.Entities;

namespace ItsGoStats
{
    public class Routing : NancyModule
    {
        public Routing()
        {
            Get["/Highlights", runAsync: true] = async (_, token) =>
            {
                return View["highlights.cshtml", new { Title = "Highlights" }];
            };

            Get["/Games", runAsync: true] = async (_, token) =>
            {
                return View["games.cshtml", new { Title = "Games" }];
            };

            Get["/Players", runAsync: true] = async (_, token) =>
            {
                var players = await DatabaseProvider.Connection.GetAllAsync<Player>();
                return View["players.cshtml", new
                {
                    Title = "Players",
                    Players = players,
                }];
            };

            Get["/"] = _ =>
            {
                return View["index.cshtml", new { Title = "Index" }];
            };
        }
    }
}
