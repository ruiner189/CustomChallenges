using ProLib.Attributes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Worldmap;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(MapController), nameof(MapController.Start))]
    [SceneModifier]
    public static class MapControllerPatch
    {
        public static void Prefix(MapController __instance)
        {
            if (ChallengeManager.ChallengeActive)
            {
                Challenge challenge = ChallengeManager.CurrentChallenge;
                if(challenge.TryGetEntryArray<String>(Keys.WHITELIST_SCENARIOS, out String[] whitelistScenarios))
                {
                    __instance._potentialRandomScenarios.RemoveAll(scenario =>
                    {
                        if (whitelistScenarios.Contains(scenario.name) || whitelistScenarios.Contains(scenario.scenarioName))
                            return false;
                        return true;
                    });
                } else if (challenge.TryGetEntryArray<String>(Keys.BLACKLIST_SCENARIOS, out String[] blacklistScenarios))
                {
                    __instance._potentialRandomScenarios.RemoveAll(scenario =>
                    {
                        if (blacklistScenarios.Contains(scenario.name) || blacklistScenarios.Contains(scenario.scenarioName))
                            return true;
                        return false;
                    });
                }

                if(challenge.TryGetEntryArray<String>(Keys.WHITELIST_BATTLES, out String[] whitelistBattles))
                {
                    RemoveBattles(__instance._potentialEasyBattles, whitelistBattles, false);
                    RemoveBattles(__instance._potentialRandomBattles, whitelistBattles, false);
                }
                else if (challenge.TryGetEntryArray<String>(Keys.BLACKLIST_BATTLES, out String[] blacklistBattles))
                {
                    RemoveBattles(__instance._potentialEasyBattles, blacklistBattles, true);
                    RemoveBattles(__instance._potentialRandomBattles, blacklistBattles, true);
                }

                if (challenge.TryGetEntryArray<String>(Keys.WHITELIST_ELITE_BATTLES, out String[] whitelistEliteBattles))
                {
                    RemoveBattles(__instance._potentialEliteBattles, whitelistEliteBattles, false);
                }
                else if (challenge.TryGetEntryArray<String>(Keys.BLACKLIST_ELITE_BATTLES, out String[] blacklistEliteBattles))
                {
                    RemoveBattles(__instance._potentialEliteBattles, blacklistEliteBattles, true);
                }

                if (__instance._loadMapData.NewGame)
                {
                    if(challenge.TryGetEntry<int>(Keys.MAX_HEALTH, out int maxHealth))
                    {
                        int health = Math.Max(maxHealth, 1);
                        __instance._playerMaxHealth.Set(health);
                        __instance._playerHealth.Set(health);
                    }
                }
            }
        }

        public static void RemoveBattles(List<MapDataBattle> battleData, String[] battles, bool blacklist)
        {
            battleData.RemoveAll(battle => {
                if (battles.Contains(battle.name))
                    return blacklist;
                return !blacklist;
            });
        }

        public static void OnSceneLoaded(String sceneName, bool firstLoad)
        {
            if (sceneName.Contains("Map"))
            {
                if (ChallengeManager.ChallengeActive)
                {
                    MapController mapController = Resources.FindObjectsOfTypeAll<MapController>().FirstOrDefault();
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    if (mapController != null)
                    {
                        if (challenge.TryGetEntryArray<String>(Keys.WHITELIST_SCENARIOS, out String[] whitelistScenarios))
                        {
                            mapController._potentialRandomScenarios.RemoveAll(scenario =>
                            {
                                if (whitelistScenarios.Contains(scenario.name) || whitelistScenarios.Contains(scenario.scenarioName))
                                    return false;
                                return true;
                            });
                        }
                        else if (challenge.TryGetEntryArray<String>(Keys.BLACKLIST_SCENARIOS, out String[] blacklistScenarios))
                        {
                            mapController._potentialRandomScenarios.RemoveAll(scenario =>
                            {
                                if (blacklistScenarios.Contains(scenario.name) || blacklistScenarios.Contains(scenario.scenarioName))
                                    return true;
                                return false;
                            });
                        }
                    }

                }
            }
        }
    }
}
