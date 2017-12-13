using Dapper.Contrib.Extensions;
using ItsGoStats.Common;
using System;

namespace ItsGoStats.Caching.Entities
{
    [Table("TeamSwitches")]
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
