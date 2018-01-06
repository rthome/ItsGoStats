using ItsGoStats.Common;

using Nancy.Routing.Constraints;

namespace ItsGoStats.Routing
{
    public class DateFormRoutingConstraint : RouteSegmentConstraintBase<DateConstraint>
    {
        public override string Name => "dateform";

        protected override bool TryMatch(string constraint, string segment, out DateConstraint matchedValue)
        {
            return DateConstraint.TryParse(segment, out matchedValue);
        }
    }
}
