using System.Collections.Generic;

using ItsGoStats.Parsing.Dto;

namespace ItsGoStats.Caching.States
{
    class RoundState
    {
        public EndOfRoundData EndOfRound { get; set; }

        public List<KillData> Kills { get; } = new List<KillData>();

        public List<AssistData> Assists { get; } = new List<AssistData>();

        public List<PurchaseData> Purchases { get; } = new List<PurchaseData>();
    }
}
