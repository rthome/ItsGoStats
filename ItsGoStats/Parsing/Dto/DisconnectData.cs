using ItsGoStats.Common;

namespace ItsGoStats.Parsing.Dto
{
    class DisconnectData : LogEventBase
    {
        public PlayerData Player { get; set; }

        public Team Team { get; set; }

        public string Reason { get; set; }
    }
}
