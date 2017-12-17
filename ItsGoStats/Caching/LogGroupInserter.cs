using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ItsGoStats.Caching.Entities;
using ItsGoStats.Parsing;
using ItsGoStats.Parsing.Dto;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using ItsGoStats.Common;
using System.Reflection;

namespace ItsGoStats.Caching
{
    class LogGroupInserter
    {
        #region State Classes

        class ServerState
        {
            public int Version { get; set; } = -1;

            public Dictionary<string, string> CVars { get; set; } = new Dictionary<string, string>();

            public int MaxRounds
            {
                get
                {
                    if (CVars.TryGetValue("mp_maxrounds", out var maxRounds))
                        return int.Parse(maxRounds);
                    return 30;
                }
            }
        }

        class GameState
        {
            public Game Game { get; set; }

            public List<DisconnectData> Disconnects { get; set; } = new List<DisconnectData>();

            public List<TeamSwitchData> TeamSwitches { get; set; } = new List<TeamSwitchData>();
        }

        class RoundState
        {
            public EndOfRoundData RoundData { get; set; }

            public List<KillData> Kills { get; set; } = new List<KillData>();

            public List<AssistData> Assists { get; set; } = new List<AssistData>();

            public List<PurchaseData> Purchases { get; set; } = new List<PurchaseData>();
        }

        #endregion

        static readonly Dictionary<string, string> weaponTranslations = new Dictionary<string, string>
        {
            { "knife_t", "knife" },
            { "knife_default_ct", "knife" },
        };

        readonly Dictionary<string, Player> playerCache = new Dictionary<string, Player>();
        readonly LogGroupParser parser;

        ServerState serverState = new ServerState();
        GameState gameState;
        RoundState roundState;

        public bool IsParsed { get; private set; }

        public bool HadError { get; private set; }

        string TranslateWeapon(string weapon)
        {
            if (weaponTranslations.TryGetValue(weapon, out var translatedWeapon))
                return translatedWeapon;
            return weapon;
        }

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

        bool ShouldFinishGame(Round round)
        {
            if (Math.Max(round.TerroristScore, round.CounterTerroristScore) > gameState.Game.MaxRounds / 2)
                return true;
            if (round.TerroristScore + round.CounterTerroristScore == gameState.Game.MaxRounds)
                return true;
            return false;
        }

        async Task FinishGameAsync(IDbConnection connection)
        {
            var roundCount = await connection.ExecuteScalarAsync<int>(@"select count(*) from Round where Round.GameId = @Id", new { gameState.Game.Id });
            if (roundCount > 0)
            {
                var lastRound = await connection.QueryFirstAsync<Round>(@"select * from Round where Round.GameId = @Id order by Round.Time desc", new { gameState.Game.Id });

                Outcome? outcome = null;
                if (lastRound.TerroristScore > gameState.Game.MaxRounds / 2)
                    outcome = Outcome.TerroristsWin;
                else if (lastRound.CounterTerroristScore > gameState.Game.MaxRounds / 2)
                    outcome = Outcome.CounterTerroristsWin;
                else if (lastRound.TerroristScore == lastRound.CounterTerroristScore && roundCount == gameState.Game.MaxRounds)
                    outcome = Outcome.Draw;

                if (outcome.HasValue)
                {
                    gameState.Game.Outcome = outcome;
                    await connection.UpdateAsync(gameState.Game);
                }
            }

            var disconnects = gameState.Disconnects.Select(data => new Disconnect
            {
                Time = data.Time,
                GameId = gameState.Game.Id,
                PlayerId = GetPlayerId(connection, data.Player),
                Team = data.Team,
                Reason = data.Reason,
            }).ToList();
            var teamswitches = gameState.TeamSwitches.Select(data => new TeamSwitch
            {
                Time = data.Time,
                GameId = gameState.Game.Id,
                PlayerId = GetPlayerId(connection, data.Player),
                PreviousTeam = data.PreviousTeam.Value,
                CurrentTeam = data.CurrentTeam.Value,
            }).ToList();

            using (var tr = connection.BeginTransaction())
            {
                await connection.InsertAsync(disconnects, tr);
                await connection.InsertAsync(teamswitches, tr);

                tr.Commit();
            }
        }

        async Task FinishRoundAsync(IDbConnection connection)
        {
            if (gameState != null)
            {
                var round = new Round
                {
                    Time = roundState.RoundData.Time,
                    GameId = gameState.Game.Id,
                    Winner = roundState.RoundData.Winner,
                    SfuiNotice = roundState.RoundData.SfuiNotice,
                    TerroristScore = roundState.RoundData.TerroristScore,
                    CounterTerroristScore = roundState.RoundData.CounterTerroristScore,
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
                    Weapon = TranslateWeapon(data.Weapon),
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

                roundState = null;
                if (ShouldFinishGame(round))
                    await FinishGameAsync(connection);
            }
        }

        #region Log Event Handlers


        Task HandleAsync(IDbConnection connection, AssistData data)
        {
            if (roundState != null)
                roundState.Assists.Add(data);
            return Task.CompletedTask;
        }

        Task HandleAsync(IDbConnection connection, CVarData data)
        {
            serverState.CVars[data.Name] = data.Value;
            return Task.CompletedTask;
        }

        Task HandleAsync(IDbConnection connection, DisconnectData data)
        {
            if (gameState != null)
                gameState.Disconnects.Add(data);
            return Task.CompletedTask;
        }

        async Task HandleAsync(IDbConnection connection, EndOfRoundData data)
        {
            if (gameState != null)
            {
                roundState.RoundData = data;
                await FinishRoundAsync(connection);

                if (gameState != null)
                    roundState = new RoundState();
            }
        }

        async Task HandleAsync(IDbConnection connection, GameStartData data)
        {
            if (gameState != null)
                await FinishGameAsync(connection);

            gameState = new GameState
            {
                Game = new Game
                {
                    Time = data.Time,
                    Map = data.Map,
                    MaxRounds = serverState.MaxRounds
                }
            };
            roundState = new RoundState();
            await connection.InsertAsync(gameState.Game);
        }

        Task HandleAsync(IDbConnection connection, KillData data)
        {
            if (roundState != null)
                roundState.Kills.Add(data);
            return Task.CompletedTask;
        }

        Task HandleAsync(IDbConnection connection, PurchaseData data)
        {
            if (roundState != null)
                roundState.Purchases.Add(data);
            return Task.CompletedTask;
        }

        Task HandleAsync(IDbConnection connection, ServerVersionData data)
        {
            serverState.Version = data.Version;
            if (data.Version < 5949)
                Error($"Server version too old: {data.Version}");
            return Task.CompletedTask;
        }

        Task HandleAsync(IDbConnection connection, TeamSwitchData data)
        {
            if (gameState != null)
                gameState.TeamSwitches.Add(data);
            return Task.CompletedTask;
        }

        #endregion

        void Error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
            HadError = true;
        }

        public async Task InsertEventsAsync(IDbConnection connection)
        {
            playerCache.Clear();
            (await connection.GetAllAsync<Player>()).AsList().ForEach(player => playerCache.Add(player.SteamId, player));

            var events = await parser.ParseAsync();
            foreach (var data in events)
            {
                // TODO: Make this less stupid - ideally find a better way to do late binding
                var method = typeof(LogGroupInserter).GetMethod("HandleAsync", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IDbConnection), data.GetType() }, null);
                await (Task)method.Invoke(this, new object[] { connection, data });
                if (HadError)
                    break;
            }
            IsParsed = true;
        }

        public LogGroupInserter(LogGroupParser parser) => this.parser = parser;
    }
}
