using System.Threading.Tasks;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Common;

namespace ItsGoStats.Models
{
    public class PlayerModel
    {
        public Player Player { get; set; }

        public static async Task<PlayerModel> CreateAsync(Player player, DateConstraint constraint)
        {
            await Task.Delay(1);

            return new PlayerModel
            {
                Player = player,
            };
        }
    }
}
