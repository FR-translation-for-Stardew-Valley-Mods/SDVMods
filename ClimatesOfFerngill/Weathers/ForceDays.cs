﻿using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

namespace ClimatesOfFerngillRebuild
{
    internal class ForceDays
    {
        private static Dictionary<SDate, int> _forceDays = new Dictionary<SDate, int>
        {
            { new SDate(1,"spring"), Game1.weather_sunny },
            { new SDate(2, "spring"), Game1.weather_sunny },
            { new SDate(3, "spring"), Game1.weather_rain },
            { new SDate(4, "spring"), Game1.weather_sunny },
            { new SDate(13, "spring"), Game1.weather_festival },
            { new SDate(24, "spring"), Game1.weather_festival },
            { new SDate(1, "summer"), Game1.weather_sunny },
            { new SDate(11, "summer"), Game1.weather_festival },
            { new SDate(13, "summer"), Game1.weather_lightning },
            { new SDate(25, "summer", 25), Game1.weather_lightning },
            { new SDate(26, "summer", 26), Game1.weather_lightning },
            { new SDate(28, "summer", 28), Game1.weather_festival },
            { new SDate(1,"fall"), Game1.weather_sunny },
            { new SDate(16,"fall"), Game1.weather_festival },
            { new SDate(27,"fall"), Game1.weather_festival },
            { new SDate(1,"winter"), Game1.weather_sunny },
            { new SDate(8, "winter"), Game1.weather_festival },
            { new SDate(25, "winter"), Game1.weather_festival }
        };

        public static bool CheckForForceDay(SDate Target, IMonitor mon, bool verbose)
        {
            foreach (KeyValuePair<SDate, int> entry in ForceDays._forceDays)
            {
                if (entry.Key.Day == Target.Day && entry.Key.Season == Target.Season)
                {
                    if (verbose) mon.Log($"Setting {entry.Value}");
                    Game1.weatherForTomorrow = entry.Value;
                    return true;
                }
            }
            return false;
        }
    }
}
