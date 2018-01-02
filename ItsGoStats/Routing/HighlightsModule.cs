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
                ViewBag.Title = "Highlights";

                return View["highlights.cshtml"];
            };
        }
    }
}
