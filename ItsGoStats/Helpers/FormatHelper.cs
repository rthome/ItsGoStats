using System.Collections.Generic;
using System.Linq;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Models;
using ItsGoStats.Parsing.Common;

using Nancy.ViewEngines.Razor;

namespace ItsGoStats.Helpers
{
    public static class FormatHelper
    {
        public static IHtmlString FormatScore(GameModel model)
        {
            return new NonEncodedHtmlString($"<span class=\"counter-terrorist\">{model.Game.FinalCounterTerroristScore}</span>&nbsp;-&nbsp;<span class=\"terrorist\">{model.Game.FinalTerroristScore}</span>");
        }

        public static IHtmlString FormatTeam(IEnumerable<Player> players, Team team, DateConstraint dateConstraint = null)
        {
            string className = null;
            switch (team)
            {
                case Team.Terrorists:
                    className = "terrorist";
                    break;
                case Team.CounterTerrorists:
                    className = "counter-terrorist";
                    break;
            }

            var links = players.Select(p => RoutingHelper.PlayerLink(p, className, dateConstraint));
            var linkList = string.Join("<br />", links.Select(l => l.ToHtmlString()));
            return new NonEncodedHtmlString(linkList);
        }
    }
}
