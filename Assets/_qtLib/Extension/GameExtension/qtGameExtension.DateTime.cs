using System;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        public static bool DateTimeCompare(this DateTime original, DateTime target)
        {
            original = new DateTime(original.Year, original.Month, original.Day, original.Hour, original.Minute, 0);
            target = new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0);
            return original.Equals(target);
        }
        
        public static long ToSeconds(this DateTime dateTime)
        {
            return dateTime.Ticks / TimeSpan.TicksPerSecond;   
        }
        
        public static DateTime GetNextDayOfWeek(DayOfWeek dayOfWeek)
        {
            DateTime today = DateTime.Today;
            int daysUntilNextMonday = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
            daysUntilNextMonday = daysUntilNextMonday == 0 ? 7 : daysUntilNextMonday; // Ensure it's next Monday, not today if today is Monday

            DateTime nextDay = today.AddDays(daysUntilNextMonday);
            return nextDay;
        }

        public static long CurrentSecond()
        {
            return DateTime.Now.ToSeconds();
        }
        
        public static long CurrentTick()
        {
            return DateTime.Now.Ticks;
        }
        
        public static DateTime Now()
        {
            return DateTime.Now;
        }

        public static DateTime Today()
        {
            return DateTime.Now.Date;
        }

        public static string ToClock(this TimeSpan timeSpan)
        {
            if ((int)timeSpan.TotalHours > 0)
            {
                return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{(int)timeSpan.TotalMinutes:D2}:{timeSpan.Seconds:D2}";
            }        
        } 
        
        public static string ToCalendar(this TimeSpan timeSpan)
        {
            if ((int)timeSpan.TotalDays > 0)
            {
                return $"{(int)timeSpan.TotalDays:D2}d {timeSpan.Hours:D2}h";
            }
            else
            {
                return $"{(int)timeSpan.TotalHours:D2}h {timeSpan.Minutes:D2}m";
            }        
        }
    }
}