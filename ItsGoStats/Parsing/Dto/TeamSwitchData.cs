using ItsGoStats.Common;

namespace ItsGoStats.Parsing.Dto
{
    class TeamSwitchData : LogEventBase
    {
        public PlayerData Player { get; set; }

        public Team PreviousTeam { get; set; }

        public Team CurrentTeam { get; set; }
    }
}
