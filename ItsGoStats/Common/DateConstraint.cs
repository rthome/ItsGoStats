using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ItsGoStats.Common
{
    public class DateConstraint
    {
        #region Static Properties

        static readonly Regex FromToRegex = new Regex(@"^/?(From/([^/]+)/To/([^/]+)/?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        static readonly (string, Func<DateTime, DateTime>)[] KnownPatterns = new(string, Func<DateTime, DateTime>)[]
        {
            ("MMMMyyyy", dt => dt.AddMonths(1)),
            ("yyyy", dt => dt.AddYears(1)),
        };

        static readonly (string, Func<DateTime>, Func<DateTime, DateTime>)[] SpecialPatterns = new(string, Func<DateTime>, Func<DateTime, DateTime>)[]
        {
            ("Today", () => DateTime.Today, dt => dt.AddDays(1)),
            ("Yesterday", () => DateTime.Today.AddDays(-1), dt => dt.AddDays(1)),
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

        public static bool TryParse(string value, out DateConstraint result)
        {
            var fromToMatch = FromToRegex.Match(value);
            if (fromToMatch.Success)
            {
                if (TryParseDateValue(fromToMatch.Groups[2].Value, out var start, out var _) && TryParseDateValue(fromToMatch.Groups[3].Value, out var _, out var end))
                {
                    result = new DateConstraint(start, end, value);
                    return true;
                }
            }
            else
            {
                if (TryParseDateValue(value, out var start, out var end))
                {
                    result = new DateConstraint(start, end, value);
                    return true;
                }
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
            UrlFragment = urlFragment;
        }
    }
}
