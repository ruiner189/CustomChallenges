using HarmonyLib;
using PeglinUI.LoadoutManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(LoadoutManager), nameof(LoadoutManager.SetupDataForNewGame))]
    public static class LoadoutPatch
    {
        public static void Prefix(LoadoutManager __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntryArray<String>(Keys.STARTING_ORBS, out String[] startingOrbs))
                {
                    List<GameObject> orbs = new List<GameObject>();
                    foreach (String orb in startingOrbs)
                    {
                        try
                        {
                            GameObject obj = Resources.Load<GameObject>($"Prefabs/Orbs/{orb}");
                            if (obj != null) orbs.Add(obj);
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError($"Failed to load the orb {orb}");
                            Plugin.Log.LogError(e.StackTrace);
                        }
                    }
                    __instance.SelectedClassLoadout.StartingOrbs = orbs;
                }
            }
        }
    }
}
