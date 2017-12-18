using System;
using System.Data;

using Dapper;

using ItsGoStats.Parsing.Common;

namespace ItsGoStats
{
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
