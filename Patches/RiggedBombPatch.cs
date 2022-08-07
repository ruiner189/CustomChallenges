using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.Start))]
    public static class RiggedBombPatch
    {
        public static void Prefix(BattleController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<int>(Keys.RIGGED_BOMB_SELF_DAMAGE, out int rigDamage))
                {
                    __instance._riggedBombSelfDamage = rigDamage;
                }
            }
        }
    }
}
