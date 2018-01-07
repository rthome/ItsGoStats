using System.Collections.Generic;

using Dapper.Contrib.Extensions;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Models;

using Nancy;

namespace ItsGoStats.Routing
{
    public class PlayersModule : NancyModule
    {
        public const string BasePath = "/Players";

        public PlayersModule()
            : base(BasePath)
        {
            Get["/", runAsync: true] = async (_, token) =>
            {
                var players = await DatabaseProvider.Connection.GetAllAsync<Player>();
                var models = new List<PlayerModel>();
                foreach (var player in players)
                    models.Add(await PlayerModel.CreateAsync(player, DateConstraint.AllTime));

                ViewBag.Date = DateConstraint.AllTime;
                return View["players.cshtml", models];
            };
        }
    }
}
