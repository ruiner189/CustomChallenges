using HarmonyLib;
using PeglinUI.MainMenu.Cruciball;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(CruciballLevelSelector), nameof(CruciballLevelSelector.UpdateCruciballSelection))]
    public static class CruciballDescriptionPatch
    {
        public static void Postfix(CruciballLevelSelector __instance)
        {
            if (ChallengeManager.ChallengeActive && __instance._selectedCruciballLevel > 0)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<DataObject>(Properties.CRUCIBALL, out DataObject cruciball)){
                    if (cruciball.TryGetEntryArray<String>(Properties.CRUCIBALL_DESCRIPTIONS, out String[] descriptions))
                    {
                        if (descriptions.Length > __instance._selectedCruciballLevel - 1)
                        {
                            __instance.currentLevelDescText.text = __instance._selectedCruciballLevel + ". " + descriptions[__instance._selectedCruciballLevel - 1];
                        }
                    }
                    if (challenge.TryGetEntry<bool>(Properties.USE_EXTERNAL_LOCALIZATION, out bool externalLocalization) && externalLocalization)
                    {

                    } 
                }
            }
        }
    }

    [HarmonyPatch(typeof(CruciballLevelSelector), nameof(CruciballLevelSelector.ToggleCruciballActive))]
    public static class CruciballUpdateDescription
    {
        public static void Prefix(CruciballLevelSelector __instance)
        {
            __instance.UpdateCruciballSelection();
        }
    }
}
