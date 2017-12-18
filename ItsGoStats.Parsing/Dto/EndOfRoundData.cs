using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Parsing.Dto
{
    public class EndOfRoundData : LogEventBase
    {
        public Team Winner { get; set; }

        public string SfuiNotice { get; set; }

        public int TerroristScore { get; set; }

        public int CounterTerroristScore { get; set; }
    }
}
