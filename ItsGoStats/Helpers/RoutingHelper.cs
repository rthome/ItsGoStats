using System;
using System.Web;
using ItsGoStats.Caching.Entities;
using ItsGoStats.Routing;

namespace ItsGoStats.Helpers
{
    public class RoutingHelper
    {
        public HtmlString PlayerLink(Player player, string cssClasses = null, string dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{PlayerModule.BasePath}/{player.SteamId}/{dateConstraint ?? ""}\">{player.Name}</a>";
            return new HtmlString(markup);
        }
    }
}
