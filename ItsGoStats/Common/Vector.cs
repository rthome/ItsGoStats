using System;
using System.Data;

using Dapper;

namespace ItsGoStats.Common
{
    class Vector
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public double Distance => Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    class VectorTypeHandler : SqlMapper.TypeHandler<Vector>
    {
        public override Vector Parse(object value)
        {
            if (value is string vectorString)
            {
                var components = vectorString.Split(';');
                return new Vector
                {
                    X = int.Parse(components[0]),
                    Y = int.Parse(components[1]),
                    Z = int.Parse(components[2]),
                };
            }
            throw new NotSupportedException();
        }

        public override void SetValue(IDbDataParameter parameter, Vector value)
        {
            var vectorString = $"{value.X};{value.Y};{value.Z}";
            parameter.Value = vectorString;
        }
    }
}
