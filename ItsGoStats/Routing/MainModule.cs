using Nancy;

namespace ItsGoStats.Routing
{
    public class MainModule : NancyModule
    {
        public const string BasePath = "";

        public MainModule()
        {           
            Get["/"] = _ =>
            {
                return View["index.cshtml"];
            };
        }
    }
}
