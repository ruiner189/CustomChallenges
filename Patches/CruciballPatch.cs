using Cruciball;
using HarmonyLib;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(CruciballManager), nameof(CruciballManager.CruciballVictoryAchieved))]
    public static class CruciballPatch
    {
        public static bool Prefix()
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(!challenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool allowCruciball) || !allowCruciball)
                {
                    return false;
                }
                return true;
            }
            return true;
        }
    }
}
