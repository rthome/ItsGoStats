using Nancy;

namespace ItsGoStats.Routing
{
    public class HighlightsModule : NancyModule
    {
        public const string BasePath = "/Highlights";

        public HighlightsModule()
            : base(BasePath)
        {
            Get["/"] = _ =>
            {
                return View["highlights.cshtml"];
            };
        }
    }
}
