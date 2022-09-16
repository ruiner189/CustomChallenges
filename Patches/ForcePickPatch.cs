using HarmonyLib;
using ProLib.Attributes;
using ProLib.Extensions;
using ProLib.Loaders;
using System;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(BattleUpgradeCanvas), nameof(PostBattleController.OnEnable))]
    public static class ForcePickPatch
    {
        public static void Postfix(BattleUpgradeCanvas __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<bool>(Properties.FORCE_POST_BATTLE_PICK, out bool forcePick) && forcePick)
                {
                    __instance._healOrText.gameObject.transform.parent.gameObject.FindChild("ORText (1)").SetActive(false);
                    __instance._healOrText.gameObject.SetActive(false);
                    __instance._upgradeHealthButton.gameObject.SetActive(false);
                    __instance._skipButton.gameObject.SetActive(false);

                    __instance._relicPanel.FindChild("SkipButton").SetActive(false);
                }
            }
        }
    }


    [HarmonyPatch(typeof(ChestScenarioController), nameof(ChestScenarioController.Update))]
    [SceneModifier]
    public static class ForceTreasurePatch
    {
        public static bool Prefix(ChestScenarioController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<bool>(Properties.FORCE_TREASURE_PICK, out bool forcePick) && forcePick)
                {
                    if (__instance._player.GetButtonDown(10))
                        return false;
                }
            }

            return true;
        }

        public static void LateOnSceneLoaded(String sceneName, bool firstLoad)
        {
            if (sceneName == SceneLoader.Treasure)
            {
                if (ChallengeManager.ChallengeActive)
                {
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    if (challenge.TryGetEntry<bool>(Properties.FORCE_TREASURE_PICK, out bool forcePick) && forcePick)
                    {
                        GameObject gameObject = GameObject.Find("Chest+Controller");
                        ChestScenarioController controller = gameObject.GetComponent<ChestScenarioController>();
                        controller.DisableSkipButton();
                    }
                }

            }
        }
    }
}
