using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;

namespace ItsGoStats.Models
{
    public class PlayerModel
    {
        public Player Player { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Assists { get; set; }

        public static async Task<PlayerModel> CreateAsync(Player player, DateConstraint constraint)
        {
            var parameters = new { player.Id, constraint.Start, constraint.End };
            var kills = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Kill where Kill.KillerId = @Id and Kill.Time >= @Start and Kill.Time < @End", parameters);
            var deaths = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Kill where Kill.VictimId = @Id and Kill.Time >= @Start and Kill.Time < @End", parameters);
            var assists = await DatabaseProvider.Connection.ExecuteScalarAsync<int>("select count(*) from Assist where Assist.AssisterId = @Id and Assist.Time >= @Start and Assist.Time < @End", parameters);

            return new PlayerModel
            {
                Player = player,
                Kills = kills,
                Deaths = deaths,
                Assists = assists,
            };
        }
    }
}
