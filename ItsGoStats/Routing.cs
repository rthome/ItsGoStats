using Nancy;

namespace ItsGoStats
{
    public class Routing : NancyModule
    {
        public Routing()
        {
            Get["/"] = _ =>
            {
                return View["index.cshtml", new { Title = "Index" }];
            };
        }
    }
}
