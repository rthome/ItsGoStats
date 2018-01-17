using System.Collections.Generic;
using System.Threading.Tasks;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Caching.Iteration;
using ItsGoStats.Common;
using ItsGoStats.Models.Analyzers;

namespace ItsGoStats.Models
{
    public class GameModel
    {
        public Game Game { get; set; }

        public List<Player> CounterTerrorists { get; set; }

        public List<Player> Terrorists { get; set; }

        public static async Task<GameModel> CreateAsync(Game game)
        {
            var stream = await EntityStream.CreateAsync(DatabaseProvider.Connection, game.Id);

            var teamHandler = new TeamHandler();
            await stream.ExecuteAsync(teamHandler);

            var counterTerrorists = await DatabaseProvider.MapPlayersAsync(teamHandler.CounterTerroristIds);
            var terrorists = await DatabaseProvider.MapPlayersAsync(teamHandler.TerroristIds);

            return new GameModel
            {
                Game = game,
                CounterTerrorists = counterTerrorists,
                Terrorists = terrorists,
            };
        }
    }
}
