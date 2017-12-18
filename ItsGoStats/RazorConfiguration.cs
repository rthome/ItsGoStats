using System.Collections.Generic;

using Nancy.ViewEngines.Razor;

namespace ItsGoStats
{
    public  class RazorConfiguration : IRazorConfiguration
    {
        public bool AutoIncludeModelNamespace => true;

        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "ItsGoStats.Parsing";
            yield return "ItsGoStats.Caching";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "ItsGoStats.Caching.Entities";
            yield return "ItsGoStats.Parsing.Common";
        }
    }
}
