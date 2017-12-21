namespace ItsGoStats.Parsing.Dto
{
    public class GameOverData : LogEventBase
    {
        public string Map { get; set; }

        public int CounterTerroristScore { get; set; }

        public int TerroristScore { get; set; }

        public int ElapsedMinutes { get; set; }
    }
}
