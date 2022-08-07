using HarmonyLib;
using Scenarios;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(DialogueScriptableObjectInteractions), nameof(DialogueScriptableObjectInteractions.DamagePlayer))]
    public static class ScenarioPatch
    {
        public static bool Prefix()
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<bool>(Keys.IMMUNE_SCENARIO_DAMAGE, out bool immuneScenarioDamage) && immuneScenarioDamage)
                    return false;

            }
            return true;
        }

        public static void Postfix(DialogueScriptableObjectInteractions __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<bool>(Keys.PERMANENT_DAMAGE, out bool permanentDamage) && permanentDamage)
                {
                    __instance.playerMaxHealth._value = __instance.playerHealth._value;
                    __instance.playerMaxHealth.Add(0);
                }
            }
        }
    }
}
