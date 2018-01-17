using System;
using System.Globalization;

namespace ItsGoStats.Common
{
    public class DateConstraint
    {
        static readonly (string, Func<DateConstraint>)[] SpecialPatterns = new (string, Func<DateConstraint>)[]
        {
            ("AllTime", () => AllTime),
            ("Today", () => Today),
            ("Yesterday", () => Yesterday),
        };

        static readonly (string, Func<DateTime, DateTime>)[] KnownPatterns = new (string, Func<DateTime, DateTime>)[]
        {
            ("MMMMyyyy", dt => dt.AddMonths(1)),
            ("yyyy", dt => dt.AddYears(1)),
        };

        public static DateConstraint AllTime => new DateConstraint(DateTime.MinValue, DateTime.MaxValue);
        public static DateConstraint Today => new DateConstraint(DateTime.Today, DateTime.Today.AddDays(1));
        public static DateConstraint Yesterday => new DateConstraint(DateTime.Today.AddDays(-1), DateTime.Today);

        public DateTime Start { get; }

        public DateTime End { get; }

        public static bool TryParse(string value, out DateConstraint result)
        {
            foreach (var (pattern, factory) in SpecialPatterns)
            {
                if (value.Equals(pattern, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = factory();
                    return true;
                }
            }

            foreach (var (pattern, factory) in KnownPatterns)
            {
                if (DateTime.TryParseExact(value, pattern, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var startDate))
                {
                    var endDate = factory(startDate);
                    result = new DateConstraint(startDate, endDate);
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static DateConstraint Merge(DateConstraint start, DateConstraint end)
        {
            var startTime = start.Start;
            var endTime = end.End;
            if (startTime < endTime)
                return new DateConstraint(startTime, endTime);
            else
                return new DateConstraint(endTime, startTime);
        }

        public string ToUrlFragment()
        {
            // TODO: Properly implement this
            var start = Start.ToString("MMMMyyyy");
            var end = End.ToString("MMMMyyyy");
            return $"From/{start}/To/{end}";
        }

        public DateConstraint(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}
