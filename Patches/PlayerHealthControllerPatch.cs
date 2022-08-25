using Battle;
using HarmonyLib;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(PlayerHealthController), nameof(PlayerHealthController.Damage))]
    public static class PlayerHealthControllerPatch
    {
        public static void Postfix(PlayerHealthController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<bool>(Properties.PERMANENT_DAMAGE, out bool permanentDamage) && permanentDamage)
                {
                    __instance._maxPlayerHealth._value = __instance._playerHealth._value;
                    __instance.UpdateHealthBar();
                }
            }
        }
    }
}
