using Cruciball;
using HarmonyLib;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(CruciballManager), nameof(CruciballManager.CruciballVictoryAchieved))]
    public static class CruciballPatch
    {
        public static bool Prefix(CruciballManager __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool allowCruciball) && allowCruciball)
                {
                    __instance._cruciballVictoryThisRun = true;
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
