using Nancy;

namespace ItsGoStats.Routing
{
    public class TeamsModule : NancyModule
    {
        public const string BasePath = "/Teams";

        public TeamsModule()
            : base(BasePath)
        {
            Get["/"] = _ =>
            {
                return View["teams.cshtml"];
            };
        }
    }
}
