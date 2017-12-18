using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Parsing.Dto
{
    public class PurchaseData : LogEventBase
    {
        public PlayerData Player { get; set; }

        public Team Team { get; set; }

        public string Item { get; set; }
    }
}
