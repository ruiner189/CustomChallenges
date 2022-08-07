using Battle.Enemies;
using HarmonyLib;
using ProLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomChallenges.Patches
{
    public static class EnemyPatch
    {
        [Register]
        public static void RegisterOnEnemyDamaged()
        {
            Enemy.OnEnemyDamaged += OnEnemyDamaged;
        }

        public static void OnEnemyDamaged(Enemy enemy, float damage)
        {
            if (ChallengeManager.ChallengeActive && enemy.CurrentHealth > 0)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if (challenge.TryGetEntry<int>(Keys.ENRAGE_THRESHOLD, out int threshold) && threshold <= damage)
                {
                    if (challenge.TryGetEntry<int>(Keys.ENRAGE_AMOUNT, out int enrage) && enrage > 0)
                    {
                        enemy.AddDamageBuff(enrage);
                    }
                }
            }
        }
    }
}
