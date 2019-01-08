using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.VR;
using UWE;

namespace RunningWithTools  
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("UpdateItemHolstering")]
    internal class Player_UpdateItemHolstering_Patch
    {

        [HarmonyPrefix]
        public static bool Player_UpdateItemHolstering_Prefix()
        {

           
                return false;
        }

    }
}
