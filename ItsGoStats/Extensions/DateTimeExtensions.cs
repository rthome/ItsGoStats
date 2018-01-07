using System;

namespace ItsGoStats.Extensions
{
    public static class DateTimeExtensions
    {
        public static string HumanizeDate(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return string.Empty;
            return dateTime.Value.HumanizeDate();
        }

        public static string HumanizeDate(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
                return "The Beginning of All Time";
            if (dateTime == DateTime.MaxValue)
                return "The End of All Time";
            if (dateTime.Date > DateTime.Today)
                return dateTime.ToString("MMMM yyyy");

            if (dateTime.Date == DateTime.Today)
                return "Today";
            if (dateTime.Date == DateTime.Today.AddDays(-1))
                return "Yesterday";
            if ((DateTime.Today - dateTime).TotalDays < 7)
                return $"Last {dateTime.DayOfWeek}";
            if (dateTime.Year == DateTime.Today.Year)
                return $"{dateTime.ToString("MMMM")} {dateTime.Day.ToOrdinal()}";
            return dateTime.ToString("MMMM yyyy");
        }
    }
}
