using System;

namespace ItsGoStats.Parsing.Common
{
    public class Vector
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public double Distance => Math.Sqrt(X * X + Y * Y + Z * Z);
    }
}
