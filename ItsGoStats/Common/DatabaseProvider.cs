using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Dapper;

using ItsGoStats.Caching;
using ItsGoStats.Caching.Entities;

namespace ItsGoStats.Common
{
    static class DatabaseProvider
    {
        public static bool IsInitialized { get; private set; }

        public static IDbConnection Connection { get; private set; }

        public static async Task InitializeAsync()
        {
            if (IsInitialized)
                return;

            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            var conn = new SQLiteConnection("Data Source=:memory:; Version=3; Foreign Keys=True; Page Size=16384");
            await conn.OpenAsync();
            await DatabaseSchema.CreateTablesAsync(conn);

            Connection = conn;
            IsInitialized = true;
        }

        public static async Task<Player> GetPlayerAsync(string steamId)
        {
            if (!IsInitialized)
                throw new InvalidOperationException($"{nameof(DatabaseProvider)} is not initialized.");

            return await Connection.QuerySingleOrDefaultAsync<Player>("select * from Player where Player.SteamId = @SteamId", new { SteamId = steamId });
        }

        public static async Task<List<Player>> MapPlayersAsync(IEnumerable<int> playerIds)
        {
            var players = await Connection.QueryAsync<Player>("select * from Player where Player.Id in @Ids", new { Ids = playerIds });
            return players.AsList();
        }
    }
}
