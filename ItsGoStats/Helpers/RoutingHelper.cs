using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Routing;

using Nancy.ViewEngines.Razor;

namespace ItsGoStats.Helpers
{
    public class RoutingHelper
    {
        public static IHtmlString PlayerLink(Player player, string cssClasses = null, DateConstraint dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{PlayerModule.BasePath}/{player.SteamId}/{dateConstraint?.ToUrlFragment()}\">{player.Name}</a>";
            return new NonEncodedHtmlString(markup);
        }
    }
}
