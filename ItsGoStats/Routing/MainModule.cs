﻿
using Nancy;

namespace ItsGoStats.Routing
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {           
            Get["/"] = _ =>
            {
                return View["index.cshtml"];
            };
        }
    }
}
