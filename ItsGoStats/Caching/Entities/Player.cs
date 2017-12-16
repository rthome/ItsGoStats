﻿using System;

using Dapper.Contrib.Extensions;

namespace ItsGoStats.Caching.Entities
{
    class Player
    {
        [Key]
        public int Id { get; set; }

        public string SteamId { get; set; }

        public DateTime NameTime { get; set; }

        public string Name { get; set; }
    }
}