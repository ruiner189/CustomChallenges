using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Worldmap;

namespace CustomChallenges.Patches
{
    [HarmonyPatch(typeof(MapNode), nameof(MapNode.SetActiveState))]
    public static class MapNodePatch
    {
        [HarmonyPatch(typeof(MapNode), nameof(MapNode.SetActiveState))]
        public static class ElitesReplaceTreasure
        {
            public static void Postfix(MapNode __instance)
            {
                if (ChallengeManager.ChallengeActive)
                {
                    Challenge challenge = ChallengeManager.CurrentChallenge;
                    bool roomTypeChanged = false;
                    if(__instance.RoomType == RoomType.BATTLE && challenge.TryGetEntry<float>(Keys.BATTLE_TO_ELITE_CONVERSION_CHANCE, out float conversionChance))
                    {
                        if(UnityEngine.Random.value < conversionChance && __instance.canBeMiniboss)
                        { 
                            __instance.RoomType = RoomType.MINI_BOSS;
                            roomTypeChanged = true;
                        }
                    }
                    else if(__instance.RoomType == RoomType.PEG_MINIGAME && challenge.TryGetEntry<bool>(Keys.PREVENT_PEG_MINIGAME, out bool preventPegMinigame) && preventPegMinigame)
                    {
                        __instance.RoomType = RoomType.SCENARIO;
                        roomTypeChanged = true;
                    }

                    if (roomTypeChanged)
                    {
                        int num = __instance.RoomType - RoomType.BATTLE;
                        for (int j = 0; j < __instance._icons.Length; j++)
                        {
                            if (j == num)
                            {
                                __instance._icons[j].SetActive(true);
                                __instance._activeIcon = __instance._icons[num].GetComponent<SpriteRenderer>().sprite;
                            }
                            else
                            {
                                __instance._icons[j].SetActive(false);
                            }
                        }
                    }

                }

            }
        }
    }
}
