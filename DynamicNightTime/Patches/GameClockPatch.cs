﻿using System;
using Microsoft.Xna.Framework;
using StardewValley;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GameClockPatch
    {
        public static void Postfix()
        {
            int sunriseTime = DynamicNightTime.GetSunrise().ReturnIntTime();
            int astronTime = DynamicNightTime.GetMorningAstroTwilight().ReturnIntTime();
            int sunsetTime = DynamicNightTime.GetSunset().ReturnIntTime();

            if (Game1.timeOfDay <= astronTime)
            {
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * .15f;

            }
            else if (Game1.timeOfDay < sunriseTime && Game1.timeOfDay >= astronTime)
            {
                if (Game1.isRaining) { 
                    float minEff = SDVTime.MinutesBetweenTwoIntTimes(astronTime, Game1.timeOfDay) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                    float percentage = (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunriseTime, astronTime));
                    Color destColor = new Color((byte)(237 - (58 * percentage)), (byte)(185 - (6 * percentage)), (byte)(74 + (105 * percentage)), (byte)(237 - (58 * percentage)));
                    //DynamicNightTime.Logger.Log($"Dest Color is {destColor}. Percentage is {percentage}. Existing color is {Game1.outdoorLight.ToString()}.");
                    Game1.outdoorLight = new Color((byte)(237 - (58 * percentage)), (byte)(185 - (6 * percentage)), (byte)(74 + (105 * percentage)), (byte)(237 - (58 * percentage)));
                }
                else
                { 
                    float minEff = SDVTime.MinutesBetweenTwoIntTimes(astronTime, Game1.timeOfDay) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                    float percentage = (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunriseTime, astronTime));
                    //means delta r is -255, delta g is -159, delta b is +175 from evening to sunrise
                    //Normal sunrise is 0,96,175. Rainy sunrises are.. 0,50,148?
                    Color destColor = new Color((byte)(255 - (255*percentage)), (byte)(255 - (159*percentage)), (byte)(175 * percentage));
                    Game1.outdoorLight = destColor;
                }
            }
            else if (Game1.timeOfDay >= sunriseTime && Game1.timeOfDay <= Game1.getStartingToGetDarkTime())
            {
                if (Game1.isRaining) 
                    { 
                        Game1.outdoorLight = Game1.ambientLight * 0.3f;
                    }
                else 
                { 
                    //Goes from [0,96,175] to [0,5,1] to [0,98,193]
                    int solarNoon = DynamicNightTime.GetSolarNoon().ReturnIntTime();
                    if (Game1.timeOfDay < solarNoon)
                    {
                        float minEff = SDVTime.MinutesBetweenTwoIntTimes(Game1.timeOfDay, sunriseTime) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                        float percentage = (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunriseTime, solarNoon));
                        Color destColor = new Color(0, (byte)(96 -(91*percentage)),(byte)(175 -(174*percentage)));
                        Game1.outdoorLight = destColor;
                    }
                    if (Game1.timeOfDay == solarNoon)
                    {
                        Color destColor = new Color(0,5,1);
                        Game1.outdoorLight = destColor;
                    }
                    if (Game1.timeOfDay > solarNoon)
                    {
                        float minEff = SDVTime.MinutesBetweenTwoIntTimes(Game1.timeOfDay, solarNoon) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                        float percentage = (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunriseTime, solarNoon));
                        Color destColor = new Color(0, (byte)(5 + (93 * percentage)), (byte)(1 + (192 * percentage)));
                        Game1.outdoorLight = destColor;
                    }
                }
            }
            else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                //Goes from [0,98,193] to [255,255,0]. We should probably space this out so that civil is still fairly bright.
                int sunset = DynamicNightTime.GetSunset().ReturnIntTime();
                int astroTwilight = DynamicNightTime.GetAstroTwilight().ReturnIntTime();
                //Color navalColor = new Color(120,178,113);
                if (Game1.isRaining)
                {
                    if (Game1.timeOfDay >= Game1.getTrulyDarkTime())
                    {
                        float num = Math.Min(0.93f, (float)(0.75 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - Game1.getTrulyDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.000624999986030161));
                        Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
                    }
                    else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
                    {
                        float num = Math.Min(0.93f, (float)(0.300000011920929 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - Game1.getStartingToGetDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.00224999990314245));
                        Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
                    }
                }
                else
                { 
                    //civil
                    if (Game1.timeOfDay > sunset && Game1.timeOfDay < astroTwilight)
                    {
                        float minEff = SDVTime.MinutesBetweenTwoIntTimes(Game1.timeOfDay, sunset) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                        float percentage = (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunset, astroTwilight));
                        Color destColor = new Color((byte)(0 + (245*percentage)), (byte)(98 + (127 * percentage)), (byte)(193 - (23 * percentage)));
                        Game1.outdoorLight = destColor;
                    }

                    //astro
                    if (Game1.timeOfDay >= astroTwilight)
                        Game1.outdoorLight = Game1.eveningColor;
                }
            }
        }
    }
}
