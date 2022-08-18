using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.BombDamage), MethodType.Getter)]
    public static class BombDamage
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref float __result)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<float>(Keys.BOMB_DAMAGE_MULTIPLIER, out float multiplier))
                {
                    __result = Mathf.RoundToInt(__result * multiplier);
                }
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RiggedBombDamage), MethodType.Getter)]
    public static class RiggedBombDamage
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref float __result)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<float>(Keys.BOMB_DAMAGE_MULTIPLIER, out float multiplier))
                {
                    __result = Mathf.RoundToInt(__result * multiplier);
                }
            }
        }
    }
}
