using HarmonyLib;
using Rewired.Integration.UnityUI;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(PopulateSuggestionOrbs), nameof(PopulateSuggestionOrbs.Start))]
    public static class PostBattleRewardPatch
    {
        public static bool Prefix(PopulateSuggestionOrbs __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                __instance._rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
                Challenge challenge = ChallengeManager.CurrentChallenge;
                bool canGetNewOrb = !challenge.TryGetEntry<bool>(Keys.PREVENT_NEW_ORBS, out bool preventOrb) || !preventOrb;
                bool canUpgradeOrb = !challenge.TryGetEntry<bool>(Keys.PREVENT_ORB_UPGRADES, out bool upgradeOrb) || !upgradeOrb;

                if (canGetNewOrb && canUpgradeOrb) 
                    return true;
                else if (!canGetNewOrb && !canUpgradeOrb)
                {
                    UpgradeOption.OnUpgradeOptionClicked?.Invoke(UpgradeOption.UpgradeType.HEALTH_HEAL, null);
                }
                else if (!canUpgradeOrb) 
                    __instance.CreateNewOrbOptions();
                else if (!canGetNewOrb && __instance.deckManager.GetUpgradeableOrbs().Count > 0) 
                    __instance.CreateUpgradeOptions();
                else
                    UpgradeOption.OnUpgradeOptionClicked?.Invoke(UpgradeOption.UpgradeType.HEALTH_HEAL, null);
                return false;
            }
            return true;
        }
    }
}
