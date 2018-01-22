using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
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

        async Task<List<PlayerModel>> CreateModelsAsync(DateConstraint constraint)
        {
            const string PlayerQuery = @"
                select Player.* from Player
                inner join (
                    select TeamSwitch.PlayerId, max(date(TeamSwitch.Time)) as Time from TeamSwitch
                    group by TeamSwitch.PlayerId) LatestTeamSwitch on Player.Id = LatestTeamSwitch.PlayerId
                where LatestTeamSwitch.Time >= @Start and LatestTeamSwitch.Time < @End";

            IEnumerable<Player> players;
            if (constraint.Start == DateTime.MinValue && constraint.End == DateTime.MaxValue)
                players = await DatabaseProvider.Connection.GetAllAsync<Player>();
            else
                players = await DatabaseProvider.Connection.QueryAsync<Player>(PlayerQuery, new { constraint.Start, constraint.End });

            var models = new List<PlayerModel>();
            foreach (var player in players)
                models.Add(await PlayerModel.CreateAsync(player, constraint));
            return models;
        }

        async Task<dynamic> ShowPlayersAsync(DateConstraint dateConstraint, CancellationToken token)
        {
            var models = await CreateModelsAsync(dateConstraint);

            ViewBag.Date = dateConstraint;
            return View["players.cshtml", models];
        }

        public PlayersModule()
            : base(BasePath)
        {
            Get["/"] = _ => Response.AsRedirect("/Players/Today");

            Get["/{Date:dateform}", runAsync: true] = async (parameters, token) => await ShowPlayersAsync((DateConstraint)parameters.Date, token);

            Get["/From/{StartDate}/To/{EndDate}", runAsync: true] = async (parameters, token) =>
            {
                if (DateConstraint.TryParse(parameters.StartDate, parameters.EndDate, out DateConstraint dateConstraint))
                    return await ShowPlayersAsync(dateConstraint, token);
                else
                    return HttpStatusCode.NotFound;
            };
        }
    }
}
