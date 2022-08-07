using Battle;
using HarmonyLib;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(PegManager), nameof(PegManager.GetPegCount))]
    public static class PegManagerPatch
    {

        public static void Postfix(Peg.PegType type, ref int __result)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(type == Peg.PegType.CRIT)
                {
                    if(challenge.TryGetEntry<int>(Keys.STARTING_CRITS, out int crits))
                    {
                        __result = __result - 2 + crits;
                    }

                } else if (type == Peg.PegType.RESET)
                {
                    if (challenge.TryGetEntry<int>(Keys.STARTING_REFRESHES, out int refreshes))
                    {
                        __result = __result - 2 + refreshes;
                    }
                }
            }
        }
    }
}
