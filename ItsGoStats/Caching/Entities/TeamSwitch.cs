using System;

using Dapper.Contrib.Extensions;

using ItsGoStats.Common;

namespace ItsGoStats.Caching.Entities
{
    [Table(nameof(TeamSwitch))]
    class TeamSwitch
    {
        [Key]
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public int GameId { get; set; }

        public int PlayerId { get; set; }

        public Team PreviousTeam { get; set; }

        public Team CurrentTeam { get; set; }
    }
}
