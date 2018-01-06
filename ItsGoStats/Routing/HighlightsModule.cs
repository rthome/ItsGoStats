using Nancy;

namespace ItsGoStats.Routing
{
    public class HighlightsModule : NancyModule
    {
        public HighlightsModule()
            : base("/Highlights")
        {
            Get["/"] = _ =>
            {
                return View["highlights.cshtml"];
            };
        }
    }
}
