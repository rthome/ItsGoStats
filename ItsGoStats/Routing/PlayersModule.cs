﻿using System;
using System.Collections.Generic;
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
            // TODO: Check player activity with team switches in the given time range
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

        public PlayersModule()
            : base(BasePath)
        {
            Get["/"] = _ =>
            {
                return Response.AsRedirect("/Players/Today");
            };

            Get["/{Date:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var models = await CreateModelsAsync((DateConstraint)parameters.Date);

                ViewBag.Date = parameters.Date;
                return View["players.cshtml", models];
            };

            Get["/From/{StartDate:dateform}/To/{EndDate:dateform}", runAsync: true] = async (parameters, token) =>
            {
                var date = DateConstraint.Merge((DateConstraint)parameters.StartDate, (DateConstraint)parameters.EndDate);
                var models = await CreateModelsAsync(date);

                ViewBag.Date = date;
                return View["players.cshtml", models];
            };
        }
    }
}
