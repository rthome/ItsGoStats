using System.Threading.Tasks;

using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Models
{
    public class GameModel
    {
        public Game Game { get; set; }

        public static async Task<GameModel> CreateAsync(Game game)
        {
            await Task.Delay(1);

            return new GameModel
            {
                Game = game,
            };
        }
    }
}
