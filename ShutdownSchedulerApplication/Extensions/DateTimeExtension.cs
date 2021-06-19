using System;

namespace ShutdownSchedulerApplication.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime RemoveSeconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0, DateTimeKind.Local);
        }
    }
}
