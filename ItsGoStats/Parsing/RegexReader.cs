using System;
using System.Text.RegularExpressions;

using ItsGoStats.Common;

namespace ItsGoStats.Parsing
{
    class RegexReader
    {
        readonly Match match;
        int index = 0;

        public bool HasMatch => match.Success;

        public int Count => match.Groups.Count;

        public string String() => match.Groups[index++].Value;

        public int Integer() => int.Parse(String());

        public DateTime Date() => new DateTime(Integer(), Integer(), Integer(), Integer(), Integer(), Integer());

        public Team Team() => (Team)Enum.Parse(typeof(Team), String());

        public Vector Vector() => new Vector { X = Integer(), Y = Integer(), Z = Integer() };

        public RegexReader(Match match) => this.match = match ?? throw new ArgumentNullException(nameof(match));
    }
}
