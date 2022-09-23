using Battle.Attacks;
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
    public static class GameInitPatch
    {
        private static List<Relic> _commonRelicPool;
        private static List<Relic> _rareRelicPool;
        private static List<Relic> _rareScenarioRelics;
        private static List<Relic> _bossRelicPool;

        private static OrbPool _gameOrbPool;

        private static int chosenRelics = 0;


        [HarmonyPatch(typeof(GameInit), nameof(GameInit.Start))]
        public static class GameInitStart
        {
            public static void Prefix(GameInit __instance)
            {
                RelicManager relicManager = __instance._relicManager;

                RevertRelicPools(relicManager);
                CopyRelicPools(relicManager);

                CopyOrbPool();
                RevertOrbPool();

                if (ChallengeManager.ChallengeActive)
                {
                    chosenRelics = 0;
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    if (challenge.ContainsKey(Properties.CRUCIBALL) && ChallengeManager.CurrentCruciballLevel > 0)
                    {
                        challenge = Challenge.GetCruciballChallenge(challenge, ChallengeManager.CurrentCruciballLevel);
                        ChallengeManager.CurrentChallenge = challenge;
                    }

                    ChallengeManager.Instance.ApplyCruciball(__instance._cruciballManager);

                    if (__instance.LoadData.NewGame)
                    {
                        if (challenge.TryGetEntryArray<string>(Properties.STARTING_ORBS, out String[] startingOrbs))
                        {
                            List<GameObject> orbs = new List<GameObject>();
                            foreach (String orb in startingOrbs)
                            {
                                try
                                {
                                    GameObject obj = Resources.Load<GameObject>($"Prefabs/Orbs/{orb}");
                                    if (obj != null) orbs.Add(obj);
                                }
                                catch (Exception e)
                                {
                                    Plugin.Log.LogError($"Failed to load the orb {orb}");
                                    Plugin.Log.LogError(e.StackTrace);
                                }
                            }
                            StaticGameData.StartingOrbs = orbs.ToArray();
                        }

                        if (challenge.TryGetEntry<int>(Properties.MAX_HEALTH, out int maxHealth))
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
                    if (challenge.TryGetEntryArray<string>(Properties.WHITELIST_RELICS, out String[] whitelistRelics))
                    {
                        List<Relic> relics = relicManager.TryGetRelics(whitelistRelics);
                        relicManager.ResetRelicPools();
                        foreach (Relic relic in relics)
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
                    else if (challenge.TryGetEntryArray<string>(Properties.BLACKLIST_RELICS, out String[] blacklistRelics))
                    {
                        relicManager.TryRemoveRelics(blacklistRelics);
                    }

                    if (challenge.TryGetEntryArray<String>(Properties.WHITELIST_ORBS, out String[] whitelistOrbs))
                    {
                        List<GameObject> orbs = RemoveOrbs(whitelistOrbs, false);
                        _gameOrbPool.AvailableOrbs = orbs.ToArray();
                    }
                    else if (challenge.TryGetEntryArray<String>(Properties.BLACKLIST_ORBS, out String[] blacklistOrbs))
                    {
                        List<GameObject> orbs = RemoveOrbs(blacklistOrbs, true);
                        _gameOrbPool.AvailableOrbs = orbs.ToArray();
                    }
                }
            }

            public static void Postfix(GameInit __instance)
            {
                if (ChallengeManager.ChallengeActive && __instance.LoadData.NewGame)
                {
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    RelicManager relicManager = __instance._relicManager;

                    if (challenge.TryGetEntryArray<string>(Properties.STARTING_RELICS, out String[] startingRelics))
                    {
                        foreach (String relicName in startingRelics)
                        {
                            if (relicManager.TryGetRelic(relicName, out Relic relic))
                            {
                                relicManager.AddRelic(relic);
                            }
                        }

                        if (__instance._chosenRelics != null && __instance._chosenRelics.Count > 0)
                        {
                            __instance._chosenRelics = new List<Relic>(__instance._relicManager.GetMultipleRelicsOfRarity(3, RelicRarity.COMMON, true));
                            for (int j = 0; j < __instance._chosenRelics.Count; j++)
                            {
                                __instance._chooseRelicIcons[j].SetRelic(__instance._chosenRelics[j]);
                            }
                        }
                    }

                    if (challenge.ContainsKey(Properties.STARTING_RELIC_RARITY))
                    {
                        RelicRarity rarity = RelicRarity.COMMON;
                        try
                        {
                            String rarityString;
                            if (challenge.IsArray(Properties.STARTING_RELIC_RARITY))
                            {
                                String[] rarities = challenge.GetEntryArray<String>(Properties.STARTING_RELIC_RARITY);
                                rarityString = rarities[0];
                            }
                            else
                            {
                                rarityString = challenge.GetEntry<String>(Properties.STARTING_RELIC_RARITY);
                            }

                            rarity = (RelicRarity)Enum.Parse(typeof(RelicRarity), rarityString.ToUpper());
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError(e.StackTrace);
                        }


                        __instance._chosenRelics = new List<Relic>(__instance._relicManager.GetMultipleRelicsOfRarity(3, rarity, true));
                        for (int k = 0; k < __instance._chosenRelics.Count; k++)
                        {
                            __instance._chooseRelicIcons[k].SetRelic(__instance._chosenRelics[k]);
                        }

                    }

                    if (challenge.TryGetEntry<bool>(Properties.SKIP_STARTING_RELIC, out bool skipStartingRelic) && skipStartingRelic)
                    {
                        __instance._chooseRelicIcons = null;
                        __instance._chosenRelics = null;
                        __instance.LoadMapScene();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameInit), nameof(GameInit.ChooseRelic))]
        public static class MultipleRelics
        {
            public static bool Prefix(GameInit __instance, int chosenIndex)
            {
                if (ChallengeManager.ChallengeActive && __instance.LoadData.NewGame)
                {
                    Challenge challenge = ChallengeManager.CurrentChallenge;

                    if(challenge.TryGetEntry<int>(Properties.STARTING_RELIC_AMOUNT, out int amountOfRelics))
                    {
                        if(chosenRelics < amountOfRelics - 1)
                        {
                            chosenRelics++;
                            RelicIcon.HideTooltip(false);
                            __instance._relicManager.AddRelic(__instance._chosenRelics[chosenIndex]);


                            RelicRarity rarity = RelicRarity.COMMON;

                            if (challenge.ContainsKey(Properties.STARTING_RELIC_RARITY))
                            {
                                String rarityString;
                                if (challenge.IsArray(Properties.STARTING_RELIC_RARITY))
                                {
                                    String[] rarities = challenge.GetEntryArray<String>(Properties.STARTING_RELIC_RARITY);
                                    int index = Math.Min(chosenRelics, rarities.Length - 1);
                                    rarityString = rarities[index];

                                } else
                                {
                                    rarityString = challenge.GetEntry<String>(Properties.STARTING_RELIC_RARITY);
                                }

                                try
                                {
                                     rarity = (RelicRarity)Enum.Parse(typeof(RelicRarity), rarityString.ToUpper());

                                }
                                catch (Exception e)
                                {
                                    Plugin.Log.LogError(e.StackTrace);
                                }

                            }
                            __instance._chosenRelics = new List<Relic>(__instance._relicManager.GetMultipleRelicsOfRarity(3, rarity, true));
                            for (int k = 0; k < __instance._chosenRelics.Count; k++)
                            {
                                __instance._chooseRelicIcons[k].SetRelic(__instance._chosenRelics[k]);
                            }

                            return false;
                        }
                    }
                }

                return true;
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
                    if (challenge.TryGetEntry<int>(Properties.STARTING_ACT, out int startingAct))
                    {
                        String scene = null;
                        if (startingAct == 1) scene = "ForestMap";
                        if (startingAct == 2) scene = "CastleMap";
                        if (startingAct == 3) scene = "MinesMap";
                        if (scene != null)
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

        [HarmonyPatch(typeof(GameInit), nameof(GameInit.SkipRelic))]
        public static class SkipStartingRelic
        {
            public static void Prefix(GameInit __instance)
            {
                if (ChallengeManager.ChallengeActive)
                {
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    RelicManager relicManager = __instance._relicManager;

                    if (challenge.TryGetEntry<bool>(Properties.PREVENT_NEW_RELICS, out bool preventRelics) && preventRelics)
                    {
                        relicManager.ResetRelicPools();
                        relicManager.SetupInternalRelicPools();
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

    }
}


