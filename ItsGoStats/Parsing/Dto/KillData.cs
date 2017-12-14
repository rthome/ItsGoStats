using ItsGoStats.Common;

namespace ItsGoStats.Parsing.Dto
{
    class KillData : LogEventBase
    {
        public PlayerData Killer { get; set; }

        public Team KillerTeam { get; set; }

        public Vector KillerPosition { get; set; }

        public PlayerData Victim { get; set; }

        public Team VictimTeam { get; set; }

        public Vector VictimPosition { get; set; }

        public bool Headshot { get; set; }

        public bool Penetrated { get; set; }

        public string Weapon { get; set; }
    }
}
