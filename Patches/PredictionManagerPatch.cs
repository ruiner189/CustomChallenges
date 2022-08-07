using HarmonyLib;
using Relics;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(PredictionManager), nameof(PredictionManager.Initialize))]
    public static class PredictionManagerPatch
    {
        public static void Postfix(PredictionManager __instance, RelicManager relicManager)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;

                if(challenge.TryGetEntry<int>(Keys.PREDICTION_BOUNCES, out int bounces))
                {
                    if (relicManager.RelicEffectActive(RelicEffect.LONGER_AIMER)) bounces++;

                    __instance._bounceCount = bounces;
                    __instance._maxIterations = __instance._baseMaxIterations * bounces;
                }
            }
        }
    }
}
