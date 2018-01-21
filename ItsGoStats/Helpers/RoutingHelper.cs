using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Routing;

using Nancy.ViewEngines.Razor;

namespace ItsGoStats.Helpers
{
    public static class RoutingHelper
    {
        public static IHtmlString PlayerLink(Player player, string cssClasses = null, DateConstraint dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{PlayerModule.BasePath}/{player.SteamId}/{dateConstraint?.UrlFragment}\">{player.Name}</a>";
            return new NonEncodedHtmlString(markup);
        }

        public static IHtmlString GameLink(Game game, string cssClasses = null, DateConstraint dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{GameModule.BasePath}/{game.Id}/{dateConstraint?.UrlFragment}\">{game.Map}</a>";
            return new NonEncodedHtmlString(markup);
        }
    }
}
