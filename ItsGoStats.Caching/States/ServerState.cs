using System.Collections.Generic;
using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Caching.States
{
    class ServerState
    {
        public int Version { get; set; } = -1;

        public Dictionary<string, string> CVars { get; } = new Dictionary<string, string>();

        public int MaxRounds
        {
            get
            {
                if (CVars.TryGetValue("mp_maxrounds", out var maxRounds))
                    return int.Parse(maxRounds);
                return 30;
            }
        }

        public void UpdateCVar(CVarData data) => CVars[data.Name] = data.Value;
    }
}
