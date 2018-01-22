using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;
using ItsGoStats.Routing;

using Nancy.ViewEngines.Razor;

namespace ItsGoStats.Helpers
{
    public static class RoutingHelper
    {
        public static string DateConstrainedLink(string baseLink, DateConstraint dateConstraint)
        {
            if (dateConstraint == null)
                return baseLink;

            var joiner = string.Empty;
            if (!baseLink.EndsWith("/"))
                joiner = "/";

            return baseLink + joiner + dateConstraint.UrlFragment;
        }

        public static IHtmlString PlayerLink(Player player, string cssClasses = null, DateConstraint dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{DateConstrainedLink($"{PlayerModule.BasePath}/{player.SteamId}", dateConstraint)}\">{player.Name}</a>";
            return new NonEncodedHtmlString(markup);
        }

        public static IHtmlString GameLink(Game game, string cssClasses = null, DateConstraint dateConstraint = null)
        {
            string classesFragment = null;
            if (cssClasses != null)
                classesFragment = $"class=\"{cssClasses}\"";
            var markup = $"<a {classesFragment} href=\"{DateConstrainedLink($"{GameModule.BasePath}/{game.Id}", dateConstraint)}\">{game.Map}</a>";
            return new NonEncodedHtmlString(markup);
        }
    }
}
