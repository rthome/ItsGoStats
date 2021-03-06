﻿using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(Disconnect))]
    public class Disconnect : IEntityWithTimestamp
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int GameId { get; set; }

        public int PlayerId { get; set; }

        public Team? Team { get; set; }

        public string Reason { get; set; }
    }
}
