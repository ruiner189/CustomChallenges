using Battle.Attacks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetDamage))]
    public static class PlayerDamagePatch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref float __result)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<float>(Properties.PLAYER_DAMAGE_MULTIPLIER, out float multiplier))
                {
                    __result = Mathf.RoundToInt(__result * multiplier);
                }
            }
        }
    }
}
