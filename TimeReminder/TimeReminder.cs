﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace TimeReminder
{
    public class TimeConfig
    {
        public int NumOfMinutes = 6;
        public bool AlertOnTheHour = true;
    }

    public class TimeReminder : Mod
    {
        public TimeConfig Config { get; set; }
        private DateTime PrevDate { get; set; }
        private bool NotTriggered = true;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<TimeConfig>();
            PrevDate = DateTime.Now;

            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            if (PrevDate.Add(new TimeSpan(0,Config.NumOfMinutes,0)) < DateTime.Now){
                Game1.hudMessages.Add(new HUDMessage("The current system time is " + DateTime.Now.ToString("h:mm:ss tt")));
                PrevDate = DateTime.Now;
            }

            if (Config.AlertOnTheHour && DateTime.Now.Minute == 0 && NotTriggered)
            {
                Game1.hudMessages.Add(new HUDMessage("The current system time is " + DateTime.Now.ToString("h:mm:ss tt")));
                NotTriggered = false;
            }

            if (DateTime.Now.Minute != 0)
                NotTriggered = true;
        }
    }
}
