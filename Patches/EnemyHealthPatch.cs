using Battle.Enemies;
using HarmonyLib;
using UnityEngine;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(Enemy), nameof(Enemy.Initialize))]
    public static class EnemyHealthPatch
    {
        public static void Postfix(Enemy __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<float>(Keys.ENEMY_HEALTH_MULTIPLIER, out float healthMultiplier))
                {
                    __instance._maxHealth = Mathf.Max(Mathf.Round(__instance._maxHealth * healthMultiplier), 1);
                    __instance.CurrentHealth = __instance._maxHealth;
                    __instance.UpdateHealthBar();
                }
            }
        }
    }
}
