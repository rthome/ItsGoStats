using System;

using Dapper.Contrib.Extensions;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Player))]
    public class Player : IEntity
    {
        [Key]
        public int Id { get; set; }

        public string SteamId { get; set; }

        public DateTime NameTime { get; set; }

        public string Name { get; set; }
    }
}
