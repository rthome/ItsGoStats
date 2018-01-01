using System;
using System.Globalization;

namespace ItsGoStats.Common
{
    public class DateForm
    {
        static readonly string[] KnownPatterns = new[]
        {
            "MMMMyyyy",
            "yyyy",
        };

        public string Pattern { get; }

        public DateTime DateTime { get; }
        
        public static bool TryParse(string value, out DateForm result)
        {
            foreach (var pattern in KnownPatterns)
            {
                if (DateTime.TryParseExact(value, pattern, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var dateTime))
                {
                    result = new DateForm(pattern, dateTime);
                    return true;
                }
            }

            result = default;
            return false;
        }

        public override string ToString() => DateTime.ToString(Pattern);

        public DateForm(string pattern, DateTime dateTime)
        {
            Pattern = pattern;
            DateTime = dateTime;
        }
    }
}
