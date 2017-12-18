using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Game))]
    class Game
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public string Map { get; set; }

        public int MaxRounds { get; set; }

        public Outcome? Outcome { get; set; }
    }
}
