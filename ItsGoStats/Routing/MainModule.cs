
using Nancy;

namespace ItsGoStats.Routing
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {           
            Get["/"] = _ =>
            {
                ViewBag.Title = "Index";

                return View["index.cshtml"];
            };
        }
    }
}
