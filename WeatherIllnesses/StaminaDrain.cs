﻿using StardewValley;
using StardewModdingAPI;
using TwilightShards.Stardew.Common;
using TwilightShards.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Locations;

namespace TwilightShards.WeatherIllnesses
{
    internal enum IllCauses
    {
        None,
        TooColdOutside,
        TooHotOutside,
        TooColdInside,
        InclementWeather,
        BlizzardsOutside,
        TheWampaWillGetYou,
        NonspecificSevereWeather
    }

    internal class StaminaDrain
    {
        private IllnessConfig IllOptions;
        private ITranslationHelper Helper;
        private bool FarmerSick;
        private IllCauses ReasonSick ;
        public bool FarmerHasBeenSick;
        private IMonitor Monitor;

        public StaminaDrain(IllnessConfig Options, ITranslationHelper SHelper, IMonitor mon)
        {
            IllOptions = Options;
            Helper = SHelper;
            Monitor = mon;
        }

        public bool IsSick()
        {
            return this.FarmerSick;
        }

        public void MakeSick()
        {
            FarmerSick = true;
            FarmerHasBeenSick = true;

            switch (ReasonSick)
            {
                case IllCauses.InclementWeather:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_inclement"),4);
                    break;
                case IllCauses.BlizzardsOutside:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_blizzard"),4);
                    break;
                case IllCauses.NonspecificSevereWeather:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_flu"),4);
                    break;
                case IllCauses.TheWampaWillGetYou:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_wampa"),4);
                    break;
                case IllCauses.TooColdInside:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_turntheheaton"), 4);
                    break;
                case IllCauses.TooColdOutside:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_cold"), 4);
                    break;
                case IllCauses.TooHotOutside:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_hot"), 4);
                    break;
                default:
                    SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_sick"),4);
                    break;
            }
        }

        public void OnNewDay()
        {
            FarmerSick = false;
            ReasonSick = IllCauses.None;
        }

        public void ClearDrain()
        {
            FarmerSick = false;
            SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_cold_removed"), 4);
        }

        public void Reset()
        {
            FarmerSick = false;
        }

        public bool FarmerCanGetSick()
        {
            if (FarmerSick && !IllOptions.SickMoreThanOnce)
                return false;

            if (!IllOptions.SickMoreThanOnce && FarmerHasBeenSick)
                return false;

            return true;
        }

        public int TenMinuteTick(int? hatID, double temp, string conditions,int ticksInHouse, int ticksOutside, int ticksTotal, MersenneTwister Dice)
        {
            double amtOutside = ticksOutside / (double)ticksTotal, totalMulti = 0;
            double amtInHouse = ticksInHouse / (double)ticksTotal;
            int staminaAffect = 0;
            var condList = new List<string>();

            if (IllOptions.Verbose)
            {
                Monitor.Log($"Ticks: {ticksOutside}/{ticksTotal} with percentage {amtOutside}:N3 against target {IllOptions.PercentageOutside}");
                Monitor.Log($"Ticks in house is {amtInHouse}:N3 against target {IllOptions.PercentageOutside}");
                Monitor.Log($"Current Condition: {conditions}");
            }
            
            //First, update the sick status
            bool farmerCaughtCold = false;
            double sickOdds = IllOptions.ChanceOfGettingSick - Game1.dailyLuck;

            //weee.
            if (hatID == 28 && (conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow")))
                sickOdds -= (Dice.NextDoublePositive() / 5.0) - .1;

            if (hatID == 25 && conditions.Contains("blizzard") || conditions.Contains("whiteout"))
                sickOdds -= .22;

            if (hatID == 4 && conditions.Contains("heatwave") && !SDVTime.IsNight)
                sickOdds -= .11;

            farmerCaughtCold = (Dice.NextDoublePositive() <= sickOdds) && (IllOptions.StaminaDrain > 0);

            FarmHouse fh = Game1.getLocationFromName("FarmHouse") as FarmHouse;
            bool isHeaterHere = false;
            foreach (var v in fh.objects.Pairs)
            {
                if (v.Value.Name.Contains("Heater"))
                    isHeaterHere = true;
            }

            bool turnTheHeatOn = (amtInHouse >= IllOptions.PercentageOutside && farmerCaughtCold &&
                                  temp < IllOptions.TooColdInside && !fh.fireplaceOn.Value && !isHeaterHere);

            if (amtOutside >= IllOptions.PercentageOutside && farmerCaughtCold || this.FarmerSick || turnTheHeatOn)
            {
                //check if it's a valid condition
                if (FarmerCanGetSick())
                {
                    //rewrite time..
                    if (conditions.Contains("blizzard") || conditions.Contains("whiteout") || (conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow")) && !(Game1.currentLocation is Desert) || (conditions.Contains("frost") && SDVTime.IsNight) || (conditions.Contains("heatwave") && !SDVTime.IsNight) || turnTheHeatOn)
                    {
                        if (turnTheHeatOn)
                            ReasonSick = IllCauses.TooColdInside;
                        else if ((conditions.Contains("heatwave") && !SDVTime.IsNight))
                            ReasonSick = IllCauses.TooHotOutside;
                        else if (conditions.Contains("frost") && SDVTime.IsNight)
                            ReasonSick = IllCauses.TooColdOutside;
                        else if (condList.Contains("blizzard"))
                            ReasonSick = IllCauses.BlizzardsOutside;
                        else if (condList.Contains("whiteout"))
                            ReasonSick = IllCauses.TheWampaWillGetYou;
                        else if (conditions.Contains("lightning") || conditions.Contains("stormy"))
                            ReasonSick = IllCauses.InclementWeather;
                        else
                            ReasonSick = IllCauses.NonspecificSevereWeather;    

                        this.MakeSick();
                    }
                }

                //now that we've done that, go through the various conditions
                if (this.FarmerSick && (conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow")))
                {
                    totalMulti += 1;
                    condList.Add("Lightning or Thundersnow");
                }

                if (this.FarmerSick && conditions.Contains("fog"))
                {
                    totalMulti += .5;
                    condList.Add("Fog");
                }

                if (this.FarmerSick && conditions.Contains("fog") && SDVTime.IsNight)
                {
                    totalMulti += .25;
                    condList.Add("Night Fog");
                }

                if (this.FarmerSick && conditions.Contains("blizzard") && !conditions.Contains("whiteout"))
                {
                    totalMulti += 1.25;
                    condList.Add("Blizzard");
                }

                if (this.FarmerSick && conditions.Contains("blizzard") && conditions.Contains("whiteout"))
                {
                    totalMulti += 2.45;
                    condList.Add("White Out");
                }

                if (this.FarmerSick && conditions.Contains("thunderfrenzy"))
                {
                    totalMulti += 1.85;
                    condList.Add("Thunder Frenzy");
                }

                if (this.FarmerSick && conditions.Contains("frost") && SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Night Frost");
                }

                if (this.FarmerSick && conditions.Contains("thundersnow") && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Thundersnow");
                }

                if (this.FarmerSick && conditions.Contains("blizzard") && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Blizzard");
                }

                if (this.FarmerSick && conditions.Contains("heatwave") && !SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Day Heatwave");
                }
            }

            staminaAffect -= (int)Math.Floor(IllOptions.StaminaDrain * totalMulti);

            if (IllOptions.Verbose && this.FarmerSick)
            {
                string condString = "[ ";
                for (int i = 0; i < condList.Count; i++)
                {
                    if (i != condList.Count - 1)
                    {
                        condString += condList[i] + ", ";
                    }
                    else
                    {
                        condString += condList[i];
                    }
                }
                condString += " ]";
                
                Monitor.Log($"[{Game1.timeOfDay}] Conditions for the drain are {condString} for a total multipler of {totalMulti} for a total drain of {staminaAffect}"); 
            }

            return staminaAffect;
        }
    }
}
