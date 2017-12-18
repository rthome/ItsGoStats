using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Parsing.Dto
{
    public class DisconnectData : LogEventBase
    {
        public PlayerData Player { get; set; }

        public Team? Team { get; set; }

        public string Reason { get; set; }
    }
}
