﻿using StardewModdingAPI.Utilities;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GetFullyDarkPatch
    {
        public static void Postfix(ref int __result)
        {
            //SDVTime calcTime = DynamicNightTime.GetNavalTwilight();
            SDVTime calcTime = new SDVTime(20, 00);
            calcTime.ClampToTenMinutes();

            __result = calcTime.ReturnIntTime();
        }
    }
}

