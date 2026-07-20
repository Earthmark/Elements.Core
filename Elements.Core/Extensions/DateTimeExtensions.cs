using System;

namespace Elements.Core
{
    public static class DateTimeExtensions
    {
        public static bool InRange(this DateTime d, DateTime startDate, DateTime endDate)
        {
            return d >= startDate && d < endDate;
        }

        public static bool InRange(this DateTimeOffset d, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return d >= startDate && d < endDate;
        }
    }
}
