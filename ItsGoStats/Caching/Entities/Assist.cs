using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Common;

namespace ItsGoStats.Caching.Entities
{
    class Assist
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
