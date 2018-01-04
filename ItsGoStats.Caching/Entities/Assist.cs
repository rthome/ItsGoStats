using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Assist))]
    public class Assist : IEntityWithTimestamp
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int RoundId { get; set; }

        public int AssisterId { get; set; }

        public Team AssisterTeam { get; set; }

        public int VictimId { get; set; }

        public Team VictimTeam { get; set; }
    }
}
