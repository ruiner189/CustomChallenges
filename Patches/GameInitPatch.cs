using CustomChallenges.Extensions;
using HarmonyLib;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(GameInit), nameof(GameInit.Start))]
    public static class GameInitPatch
    {
        private static List<Relic> _commonRelicPool;
        private static List<Relic> _rareRelicPool;
        private static List<Relic> _rareScenarioRelics;
        private static List<Relic> _bossRelicPool;

        private static List<GameObject> _orbPool;
        private static OrbPool _gameOrbPool;

        public static void Prefix(GameInit __instance)
        {
            RelicManager relicManager = __instance._relicManager;

            RevertRelicPools(relicManager);
            CopyRelicPools(relicManager);

            CopyOrbPool();
            RevertOrbPool();

            if (ChallengeManager.ChallengeActive && __instance.LoadData.NewGame)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;

                if (challenge.TryGetEntryArray<string>(Keys.STARTING_ORBS, out String[] startingOrbs))
                {
                    List<GameObject> orbs = new List<GameObject>();
                    foreach(String orb in startingOrbs)
                    {
                        try
                        {
                            GameObject obj = Resources.Load<GameObject>($"Prefabs/Orbs/{orb}");
                            if (obj != null) orbs.Add(obj);
                        } catch(Exception e)
                        {
                            Plugin.Log.LogError($"Failed to load the orb {orb}");
                            Plugin.Log.LogError(e.StackTrace);
                        }
                    }
                    __instance._initialDeck.Balls = orbs;
                }

                if (challenge.TryGetEntryArray<string>(Keys.WHITELIST_RELICS, out String[] whitelistRelics))
                {
                    List<Relic> relics = relicManager.TryGetRelics(whitelistRelics);
                    relicManager.ResetRelicPools();
                    foreach(Relic relic in relics)
                    {
                        switch (relic.globalRarity)
                        {
                            case RelicRarity.COMMON:
                                relicManager.CommonRelicPool.Add(relic);
                                break;
                            case RelicRarity.RARE:
                                relicManager.RareRelicPool.Add(relic);
                                break;
                            case RelicRarity.BOSS:
                                relicManager.BossRelicPool.Add(relic);
                                break;
                            case RelicRarity.NONE:
                                relicManager.RareScenarioRelicPool.Add(relic);
                                break;
                        }
                    }
                    relicManager.SetupInternalRelicPools();
                }
                else if (challenge.TryGetEntryArray<string>(Keys.BLACKLIST_RELICS, out String[] blacklistRelics))
                {
                    relicManager.TryRemoveRelics(blacklistRelics);
                }

                if(challenge.TryGetEntryArray<String>(Keys.WHITELIST_ORBS, out String[] whitelistOrbs))
                {
                    List<GameObject> orbs = RemoveOrbs(whitelistOrbs, false);
                    _gameOrbPool.AvailableOrbs = orbs.ToArray();
                } else if (challenge.TryGetEntryArray<String>(Keys.BLACKLIST_ORBS, out String[] blacklistOrbs))
                {
                    List<GameObject> orbs = RemoveOrbs(blacklistOrbs, true);
                    _gameOrbPool.AvailableOrbs = orbs.ToArray();
                }

                if(challenge.TryGetEntry<int>(Keys.MAX_HEALTH, out int maxHealth))
                {
                    int health = Math.Max(maxHealth, 1);
                    if (__instance.maxPlayerHealth != null)
                    {
                        __instance.maxPlayerHealth.Set(health);
                    }
                    if (__instance.playerHealth != null)
                    {
                        __instance.playerHealth.Set(health);
                    }
                }
            } 
        }

        private static void CopyRelicPools(RelicManager relicManager)
        {
            _commonRelicPool = new List<Relic>(relicManager.CommonRelicPool);
            _rareRelicPool = new List<Relic>(relicManager.RareRelicPool);
            _rareScenarioRelics = new List<Relic>(relicManager.RareScenarioRelicPool);
            _bossRelicPool = new List<Relic>(relicManager.BossRelicPool);
        }

        private static void RevertRelicPools(RelicManager relicManager)
        {
            if (_commonRelicPool != null) relicManager.CommonRelicPool = _commonRelicPool;
            if (_rareRelicPool != null) relicManager.RareRelicPool = _rareRelicPool;
            if (_rareScenarioRelics != null) relicManager.RareScenarioRelicPool = _rareScenarioRelics;
            if (_bossRelicPool != null) relicManager.BossRelicPool = _bossRelicPool;
        }

        private static void CopyOrbPool()
        {
            OrbPool[] pools = Resources.FindObjectsOfTypeAll<OrbPool>();
            if (pools.Length >= 1)
            {
                _gameOrbPool = pools[0];
            }
        }

        private static void RevertOrbPool()
        {
            if(_gameOrbPool != null)
            {
                _gameOrbPool.AvailableOrbs = _gameOrbPool._availableOrbs.ToArray();
            }
        }

        private static List<GameObject> RemoveOrbs(String[] orbNames, bool blacklist)
        {
            List<GameObject> orbs = new List<GameObject>(_gameOrbPool.AvailableOrbs);
            orbs.RemoveAll(orb => {
                String orbName = orb.name;
                if (orbNames.Contains(orb.name))
                    return blacklist;

                // Just in case there is a clone here from mods
                orbName = orbName.Replace("(clone)", "").Trim();
                if (orbNames.Contains(orb.name))
                    return blacklist;

                // Last attempt on gameobject name. Removing the lvl part of the name, as they should all be level one.
                orbName = Regex.Replace(orbName, "-Lvl1","");
                if (orbNames.Contains(orb.name))
                    return blacklist;

                // Now trying if the name matches in the attack
                Attack attack = orb.GetComponent<Attack>();
                if (attack != null)
                {
                    if (orbNames.Contains(attack.locNameString) || orbNames.Contains(attack.locName))
                        return blacklist;
                }

                return !blacklist;
            });

            if (orbs.Count == 2 || orbs.Count == 3) orbs.AddRange(orbs);
            return orbs;
        }

        public static void Postfix(GameInit __instance)
        {
            if (ChallengeManager.ChallengeActive && __instance.LoadData.NewGame)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                RelicManager relicManager = __instance._relicManager;

                if(challenge.TryGetEntryArray<string>(Keys.STARTING_RELICS, out String[] startingRelics))
                {
                    foreach(String relicName in startingRelics)
                    {
                        if(relicManager.TryGetRelic(relicName, out Relic relic))
                        {
                            relicManager.AddRelic(relic);
                        }
                    }

                    if(__instance._chosenRelics != null && __instance._chosenRelics.Count > 0)
                    {
                        __instance._chosenRelics = new List<Relic>(__instance._relicManager.GetMultipleRelicsOfRarity(3, RelicRarity.COMMON, true));
                        for (int j = 0; j < __instance._chosenRelics.Count; j++)
                        {
                            __instance._chooseRelicIcons[j].SetRelic(__instance._chosenRelics[j]);
                        }
                    }
                }

                if(challenge.TryGetEntry<bool>(Keys.SKIP_STARTING_RELIC, out bool skipStartingRelic) && skipStartingRelic){
                    __instance._chooseRelicIcons = null;
                    __instance._chosenRelics = null;
                    __instance.LoadMapScene();
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameInit), nameof(GameInit.LoadMapScene))]
    public static class ChangeStartingScene
    {
        public static bool Prefix()
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntry<int>(Keys.STARTING_ACT, out int startingAct))
                {
                    String scene = null;
                    if (startingAct == 1) scene = "ForestMap";
                    if (startingAct == 2) scene = "CastleMap";
                    if (startingAct == 3) scene = "MinesMap";
                    if(scene != null)
                    {
                        SceneManager.LoadScene(scene);
                        return false;
                    }
                    return true;
                }
            }
            return true;
        }
    }
}


