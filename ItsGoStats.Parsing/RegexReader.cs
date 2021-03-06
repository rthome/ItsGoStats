﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats.Parsing
{
    public class RegexReader
    {
        static readonly Dictionary<string, Team> TeamMapping = new Dictionary<string, Common.Team>
        {
            {"CT", Common.Team.CounterTerrorists },
            {"TERRORIST", Common.Team.Terrorists },
            {"Unassigned", Common.Team.Unassigned },
            {"Spectator", Common.Team.Spectators },
        };

        readonly string[] matchGroups;
        int index = 1;

        public int Count => matchGroups.Length;

        static Team? MapTeamString(string value)
        {
            if (TeamMapping.TryGetValue(value, out var team))
                return team;
            else
            {
                if (string.IsNullOrEmpty(value))
                    return null;
                throw new ArgumentException($"Unknown team string '{value}'", nameof(value));
            }
        }

        public string String() => matchGroups[index++];

        public int Integer() => int.Parse(String());

        public DateTime Date()
        {
            var month = Integer();
            var day = Integer();
            var year = Integer();
            var hour = Integer();
            var minute = Integer();
            var second = Integer();
            return new DateTime(year, month, day, hour, minute, second);
        }

        public Team? Team() => MapTeamString(String());

        public Vector Vector()
        {
            var x = Integer();
            var y = Integer();
            var z = Integer();
            return new Vector { X = x, Y = y, Z = z };
        }

        public RegexReader(Match match) => matchGroups = match.Groups.OfType<Group>().Select(g => g.Value).ToArray();
    }
}
