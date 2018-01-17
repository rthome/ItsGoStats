using System.Collections.Generic;
using System.Linq;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Caching.Iteration;
using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Models.Analyzers
{
    class TeamHandler : EntityHandler
    {
        readonly Dictionary<int, Team> playerTeamMap = new Dictionary<int, Team>();

        public List<int> TerroristIds => playerTeamMap.Where(kv => kv.Value == Team.Terrorists).Select(kv => kv.Key).ToList();

        public List<int> CounterTerroristIds => playerTeamMap.Where(kv => kv.Value == Team.CounterTerrorists).Select(kv => kv.Key).ToList();

        public override void OnStart()
        {
            playerTeamMap.Clear();
        }

        public override void OnTeamSwitch(int index, TeamSwitch entity)
        {
            playerTeamMap[entity.PlayerId] = entity.CurrentTeam;
        }
    }
}
