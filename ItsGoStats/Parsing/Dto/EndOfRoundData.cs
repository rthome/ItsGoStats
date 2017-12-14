using ItsGoStats.Common;

namespace ItsGoStats.Parsing.Dto
{
    class EndOfRoundData : LogEventBase
    {
        public Team Winner { get; set; }

        public string SfuiNotice { get; set; }

        public int TerroristScore { get; set; }

        public int CounterTerroristScore { get; set; }
    }
}
