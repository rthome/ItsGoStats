using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItsGoStats.Caching.Entities
{
    class Player
    {
        public int Id { get; set; }

        public string SteamId { get; set; }

        public string Name { get; set; }

        public DateTime NameTime { get; set; }
    }
}
