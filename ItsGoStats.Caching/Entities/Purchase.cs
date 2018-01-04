using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Purchase))]
    public class Purchase : IEntityWithTimestamp
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int RoundId { get; set; }

        public int PlayerId { get; set; }

        public Team Team { get; set; }

        public string Item { get; set; }
    }
}
