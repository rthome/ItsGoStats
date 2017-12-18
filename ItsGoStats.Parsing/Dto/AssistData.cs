using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Parsing.Dto
{
    public class AssistData : LogEventBase
    {
        public PlayerData Assister { get; set; }

        public Team AssisterTeam { get; set; }

        public PlayerData Victim { get; set; }

        public Team VictimTeam { get; set; }
    }
}
