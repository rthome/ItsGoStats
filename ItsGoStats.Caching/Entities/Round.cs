using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Round))]
    public class Round : IEntityWithTimestamp
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int GameId { get; set; }

        public Team Winner { get; set; }

        public string SfuiNotice { get; set; }

        public int TerroristScore { get; set; }

        public int CounterTerroristScore { get; set; }
    }
}
