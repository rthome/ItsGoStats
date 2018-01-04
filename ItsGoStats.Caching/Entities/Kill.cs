using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Kill))]
    public class Kill : IEntityWithTimestamp
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int RoundId { get; set; }

        public int KillerId { get; set; }

        public Team KillerTeam { get; set; }

        public Vector KillerPosition { get; set; }

        public int VictimId { get; set; }

        public Team VictimTeam { get; set; }

        public Vector VictimPosition { get; set; }

        public bool Headshot { get; set; }

        public bool Penetrated { get; set; }

        public string Weapon { get; set; }
    }
}
