using StardewValley;
using System;

namespace SDVFactory.Factory
{
    public struct LoreTime
    {
        public int Minutes;
        public int Day;
        public int Month;
        public int Year;

        public uint TotalMinutes
        {
            get
            {
                uint r = (uint)Minutes;
                r += (uint)Day * 1440;
                r += (uint)Month * 40320;
                r += (uint)Year * 161280;
                return r;
            }
        }

        public static LoreTime Zero { get { return new LoreTime(); } }

        public static LoreTime Now { get
            {
                int minutes = MinutesSoFarToday();
                int day = Game1.dayOfMonth;
                int month = Utility.getSeasonNumber(Game1.currentSeason);
                int year = Game1.year;
                return new LoreTime() { Year = year, Month = month, Day = day, Minutes = minutes };
            } 
        }

        //return the difference between this time and the other in game minutes
        //accounts for things like days, months, years. accounts for time not actually playable, e.g. 2am-6am as well.
        public static uint operator - (LoreTime a, LoreTime b) => a.TotalMinutes - b.TotalMinutes;
        public static uint operator + (LoreTime a, LoreTime b) => a.TotalMinutes + b.TotalMinutes;

        //returns the number of lore minutes passed so far since 12am
        private static int MinutesSoFarToday()
        {
            int tod = GetTimeOfDay();
            int hours = tod / 100;
            return (hours * 60) + (tod % 100);
        }

        //returns time of day 24h clock integer but without rounding to 10 minutes
        private static int GetTimeOfDay()
        {
            int num = (int)Math.Floor(Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / 7000f * 10f);
            return Utility.ConvertMinutesToTime(num);
        }
    }
}
