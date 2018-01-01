using Nancy.Routing.Constraints;

namespace ItsGoStats.Common
{
    public class DateFormRoutingConstraint : RouteSegmentConstraintBase<DateForm>
    {
        public override string Name => "dateform";

        protected override bool TryMatch(string constraint, string segment, out DateForm matchedValue)
        {
            return DateForm.TryParse(segment, out matchedValue);
        }
    }
}
