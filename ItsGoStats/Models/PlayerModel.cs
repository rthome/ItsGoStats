using System;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;

namespace ItsGoStats.Models
{
    public class PlayerModel
    {
        /// <summary>
        /// The Player entity
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// Date and time when the player was first seen (all time)
        /// </summary>
        public DateTime? FirstSeen { get; set; }

        /// <summary>
        /// Date and time when the player was last seen (all time)
        /// </summary>
        public DateTime? LastSeen { get; set; }

        /// <summary>
        /// Number of kills the player has
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// Number of deaths the player has
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// Number of assists the player has
        /// </summary>
        public int Assists { get; set; }

        public static async Task<PlayerModel> CreateAsync(Player player, DateConstraint constraint)
        {
            var parameters = new { player.Id, constraint.Start, constraint.End };
            var kills = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Kill where Kill.KillerId = @Id and Kill.Time >= @Start and Kill.Time < @End", parameters);
            var deaths = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Kill where Kill.VictimId = @Id and Kill.Time >= @Start and Kill.Time < @End", parameters);
            var assists = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Assist where Assist.AssisterId = @Id and Assist.Time >= @Start and Assist.Time < @End", parameters);

            // TODO: Check player activity with team switches during the given time range
            var firstSeen = await DatabaseProvider.Connection.QueryFirstOrDefaultAsync<DateTime?>("select date(Time) from TeamSwitch where TeamSwitch.PlayerId = @Id order by TeamSwitch.Time asc limit 1", parameters);
            var lastSeen = await DatabaseProvider.Connection.QueryFirstOrDefaultAsync<DateTime?>("select date(Time) from TeamSwitch where TeamSwitch.PlayerId = @Id order by TeamSwitch.Time desc limit 1", parameters);

            return new PlayerModel
            {
                Player = player,
                FirstSeen = firstSeen,
                LastSeen = lastSeen,
                Kills = kills,
                Deaths = deaths,
                Assists = assists,
            };
        }
    }
}
