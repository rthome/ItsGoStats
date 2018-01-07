namespace ItsGoStats.Extensions
{
    public static class IntegerExtensions
    {
        public static string ToOrdinal(this int i)
        {
            var number = i.ToString();

            if (i <= 0)
                return number;
            switch(i % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }
            switch (i % 10)
            {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }
    }
}
