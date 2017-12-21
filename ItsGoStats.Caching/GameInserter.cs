using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Dapper;
using Dapper.Contrib.Extensions;

using ItsGoStats.Caching.Entities;
using ItsGoStats.Caching.States;
using ItsGoStats.Parsing.Common;
using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Caching
{
    class GameInserter
    {
        readonly Dictionary<Type, MethodInfo> handlerMethodCache = new Dictionary<Type, MethodInfo>();
        Dictionary<string, Player> playerCache;

        GameState gameState;
        RoundState currentRoundState;
        readonly List<RoundState> previousRoundStates = new List<RoundState>();

        ServerState ServerState { get; }

        // TODO: Make async?
        int GetPlayerId(IDbConnection connection, PlayerData data)
        {
            if (!playerCache.TryGetValue(data.SteamId, out var player))
            {
                var created = false;
                player = connection.QuerySingleOrDefault<Player>("select * from Player where Player.SteamId = @SteamId", new { data.SteamId });
                if (player == null)
                {
                    player = new Player { SteamId = data.SteamId, NameTime = data.NameTime, Name = data.Name };
                    connection.Insert(player);
                    created = true;
                }

                playerCache[player.SteamId] = player;
                if (created)
                    return player.Id;
            }

            if (data.NameTime > player.NameTime && data.Name != player.Name)
            {
                player.NameTime = data.NameTime;
                player.Name = data.Name;
                connection.Update(player);
            }

            return player.Id;
        }

        Outcome CalculateOutcome(int ctScore, int tScore, int maxRounds)
        {
            if (tScore > maxRounds / 2)
                return Outcome.TerroristsWin;
            else if (ctScore > maxRounds / 2)
                return Outcome.CounterTerroristsWin;
            return Outcome.Draw;
        }

        async Task InsertRoundStateAsync(IDbConnection connection, Game game, RoundState roundState)
        {
            var round = new Round
            {
                Time = roundState.EndOfRound.Time,
                GameId = game.Id,
                Winner = roundState.EndOfRound.Winner,
                SfuiNotice = roundState.EndOfRound.SfuiNotice,
                TerroristScore = roundState.EndOfRound.TerroristScore,
                CounterTerroristScore = roundState.EndOfRound.CounterTerroristScore,
            };
            await connection.InsertAsync(round);

            var kills = roundState.Kills.Select(data => new Kill
            {
                Time = data.Time,
                RoundId = round.Id,
                KillerId = GetPlayerId(connection, data.Killer),
                KillerTeam = data.KillerTeam,
                KillerPosition = data.KillerPosition,
                VictimId = GetPlayerId(connection, data.Victim),
                VictimTeam = data.VictimTeam,
                VictimPosition = data.VictimPosition,
                Headshot = data.Headshot,
                Penetrated = data.Penetrated,
                Weapon = data.Weapon,
            }).ToList();
            var assists = roundState.Assists.Select(data => new Assist
            {
                Time = data.Time,
                RoundId = round.Id,
                AssisterId = GetPlayerId(connection, data.Assister),
                AssisterTeam = data.AssisterTeam,
                VictimId = GetPlayerId(connection, data.Victim),
                VictimTeam = data.VictimTeam,
            }).ToList();
            var purchases = roundState.Purchases.Select(data => new Purchase
            {
                Time = data.Time,
                RoundId = round.Id,
                PlayerId = GetPlayerId(connection, data.Player),
                Team = data.Team,
                Item = data.Item,
            }).ToList();

            using (var tr = connection.BeginTransaction())
            {
                if (kills.Any())
                    await connection.InsertAsync(kills, tr);
                if (assists.Any())
                    await connection.InsertAsync(assists, tr);
                if (purchases.Any())
                    await connection.InsertAsync(purchases, tr);

                tr.Commit();
            }
        }

        async Task<Game> InsertGameStateAsync(IDbConnection connection)
        {
            var game = new Game
            {
                Time = gameState.GameOver.Time,
                Map = gameState.GameOver.Map,
                ElapsedMinutes = gameState.GameOver.ElapsedMinutes,
                FinalCounterTerroristScore = gameState.GameOver.CounterTerroristScore,
                FinalTerroristScore = gameState.GameOver.TerroristScore,
                Outcome = CalculateOutcome(gameState.GameOver.CounterTerroristScore, gameState.GameOver.TerroristScore, ServerState.MaxRounds),
            };
            await connection.InsertAsync(game);

            foreach (var roundState in previousRoundStates)
                await InsertRoundStateAsync(connection, game, roundState);

            var disconnects = gameState.Disconnects.Select(data => new Disconnect
            {
                Time = data.Time,
                GameId = game.Id,
                PlayerId = GetPlayerId(connection, data.Player),
                Team = data.Team,
                Reason = data.Reason,
            }).ToList();
            var teamswitches = gameState.TeamSwitches.Select(data => new TeamSwitch
            {
                Time = data.Time,
                GameId = game.Id,
                PlayerId = GetPlayerId(connection, data.Player),
                PreviousTeam = data.PreviousTeam.Value,
                CurrentTeam = data.CurrentTeam.Value,
            }).ToList();

            using (var tr = connection.BeginTransaction())
            {
                if (disconnects.Any())
                    await connection.InsertAsync(disconnects, tr);
                if (teamswitches.Any())
                    await connection.InsertAsync(teamswitches, tr);

                tr.Commit();
            }

            return game;
        }

        #region Log Event Handlers

        void Handle(AssistData data)
        {
            currentRoundState.Assists.Add(data);
        }

        void Handle(DisconnectData data)
        {
            gameState.Disconnects.Add(data);
        }

        void Handle(EndOfRoundData data)
        {
            currentRoundState.EndOfRound = data;
            previousRoundStates.Add(currentRoundState);

            currentRoundState = new RoundState();
        }

        void Handle(GameOverData data)
        {
            gameState.GameOver = data;
        }

        void Handle(GameStartData data)
        {
            gameState = new GameState
            {
                GameStart = data,
            };

            currentRoundState = new RoundState();
        }

        void Handle(KillData data)
        {
            currentRoundState.Kills.Add(data);
        }

        void Handle(PurchaseData data)
        {
            currentRoundState.Purchases.Add(data);
        }

        void Handle(TeamSwitchData data)
        {
            gameState.TeamSwitches.Add(data);
        }

        #endregion

        async Task InitializePlayerCacheAsync(IDbConnection connection)
        {
            var players = (await connection.GetAllAsync<Player>()).AsList();
            playerCache = players.ToDictionary(p => p.SteamId);
        }

        public async Task<Game> CreateGameAsync(IDbConnection connection, IReadOnlyList<LogEventBase> events)
        {
            await InitializePlayerCacheAsync(connection);

            foreach (var data in events)
            {
                var dataType = data.GetType();
                if (!handlerMethodCache.TryGetValue(dataType, out var handlerMethod))
                {
                    handlerMethod = GetType().GetMethod(nameof(Handle), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { dataType }, null);
                    handlerMethodCache.Add(dataType, handlerMethod);
                }

                handlerMethod?.Invoke(this, new[] { data });
            }

            return await InsertGameStateAsync(connection);
        }

        public GameInserter(ServerState serverState)
        {
            ServerState = serverState;
        }
    }
}
