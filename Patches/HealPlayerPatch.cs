using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.TriggerVictory))]
    public static class HealPlayerPatch
    {
        public static void Prefix(BattleController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<bool>(Properties.FULL_HEAL_AT_END_OF_BATTLE, out bool shouldHeal) && shouldHeal)
                {
                    __instance._playerHealthController.HealToFull();
                }
            }
        }
    }
}
