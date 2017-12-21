using System.Collections.Generic;

using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Caching.States
{
    class GameState
    {
        public GameStartData GameStart { get; set; }

        public GameOverData GameOver { get; set; }

        public List<DisconnectData> Disconnects { get; } = new List<DisconnectData>();

        public List<TeamSwitchData> TeamSwitches { get; } = new List<TeamSwitchData>();
    }
}
