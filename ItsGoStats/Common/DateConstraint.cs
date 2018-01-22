using System;
using System.Globalization;

namespace ItsGoStats.Common
{
    public class DateConstraint
    {
        #region Static Properties

        static readonly (string, Func<DateTime, DateTime>)[] KnownPatterns = new(string, Func<DateTime, DateTime>)[]
        {
            ("MMMMyyyy", dt => dt.AddMonths(1)),
            ("yyyy", dt => dt.AddYears(1)),
        };

        static readonly (string, Func<DateTime>, Func<DateTime, DateTime>)[] SpecialPatterns = new(string, Func<DateTime>, Func<DateTime, DateTime>)[]
        {
            ("Today", () => DateTime.Today, dt => dt.AddDays(1)),
            ("Yesterday", () => DateTime.Today.AddDays(-1), dt => dt.AddDays(1)),
            ("AllTime", () => DateTime.MinValue, _ => DateTime.MaxValue),
        };

        static bool TryParseDateValue(string value, out DateTime start, out DateTime end)
        {
            foreach (var (pattern, startFunc, endFunc) in SpecialPatterns)
            {
                if (pattern.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    start = startFunc();
                    end = endFunc(start);
                    return true;
                }
            }

            foreach (var (pattern, endFunc) in KnownPatterns)
            {
                if (DateTime.TryParseExact(value, pattern, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out start))
                {
                    end = endFunc(start);
                    return true;
                }
            }

            start = default;
            end = default;
            return false;
        }

        public static bool TryParse(string from, string to, out DateConstraint result)
        {
            if (TryParseDateValue(from, out var start, out var _) && TryParseDateValue(to, out var _, out var end))
            {
                result = new DateConstraint(start, end, $"From/{from}/To/{to}");
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryParse(string value, out DateConstraint result)
        {
            if (TryParseDateValue(value, out var start, out var end))
            {
                result = new DateConstraint(start, end, value);
                return true;
            }

            result = default;
            return false;
        }

        #endregion

        public DateTime Start { get; }

        public DateTime End { get; }

        public string UrlFragment { get; }

        public DateConstraint(DateTime start, DateTime end, string urlFragment)
        {
            Start = start.Date;
            End = end.Date;
            UrlFragment = urlFragment.TrimStart('/');
        }
    }
}
