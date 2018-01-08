using System;

namespace ItsGoStats.Extensions
{
    public static class DateTimeExtensions
    {
        public static string HumanizeDate(this DateTime? dateTime, bool shortForm = false)
        {
            if (!dateTime.HasValue)
                return string.Empty;
            return dateTime.Value.HumanizeDate(shortForm);
        }

        public static string HumanizeDate(this DateTime dateTime, bool shortForm = false)
        {
            if (dateTime == DateTime.MinValue)
            {
                if (shortForm)
                    return "The Beginning";
                else
                    return "The Beginning of All Time";
            }
            if (dateTime == DateTime.MaxValue)
            {
                if (shortForm)
                    return "The End";
                else
                    return "The End of All Time";
            }
            if (dateTime.Date > DateTime.Today)
            {
                if (shortForm)
                    return dateTime.ToString(@"MMM\. yyyy");
                else
                    return dateTime.ToString("MMMM yyyy");
            }

            if (dateTime.Date == DateTime.Today)
                return "Today";
            if (dateTime.Date == DateTime.Today.AddDays(-1))
                return "Yesterday";
            if ((DateTime.Today - dateTime.Date).TotalDays < 6)
                return $"{(int)(DateTime.Today - dateTime.Date).TotalDays} days ago";
            if ((dateTime.Year == DateTime.Today.Year)  || (DateTime.Today - dateTime.Date).TotalDays < 90)
            {
                if (shortForm)
                    return $"{dateTime.ToString(@"MMM\.")} {dateTime.Day.ToOrdinal()}";
                else
                    return $"{dateTime.ToString("MMMM")} {dateTime.Day.ToOrdinal()}";
            }
            if (shortForm)
                return dateTime.ToString(@"MMM\. yyyy");
            else
                return dateTime.ToString("MMMM yyyy");
        }
    }
}
