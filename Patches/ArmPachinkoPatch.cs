using Battle.Pachinko.BallBehaviours;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.ArmBallForShot))]
    public static class ArmPachinkoPatch
    {
        public static void Postfix(BattleController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<bool>(Properties.ORB_DESTROYS_PEG, out bool destroys) && destroys)
                {
                    DestroyPegsOnHit component = __instance.activePachinkoBall.GetComponent<DestroyPegsOnHit>();
                    if (component == null)
                        __instance.activePachinkoBall.AddComponent<DestroyPegsOnHit>();
                }
            }
        }
    }
}
