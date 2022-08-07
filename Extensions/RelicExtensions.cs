using ProLib.Relics;
using Relics;
using System;
using System.Collections.Generic;

namespace CustomChallenges.Extensions
{
    public static class RelicExtensions
    {
        public static bool TryGetRelic(this RelicManager relicManager, String relicName, out Relic relic)
        {
            List<Relic> relics = relicManager._globalRelics._relics;

            // Search vanilla names or ID numbers
            if (Enum.TryParse<RelicEffect>(relicName, false, out RelicEffect relicEffect))
            {
                relic = relics.Find(r => r.effect == relicEffect);
                if (relic != null) return true;
            }

            // Search ElementalCore
            if (CustomRelic.TryGetCustomRelic(relicName, out CustomRelic customRelic))
            {
                relic = customRelic;
                return true;
            }

            // Search using RelicLib
            if (ChallengeManager.LoadedMods.Contains("io.github.crazyjackel.RelicLib"))
            {
                object[] args = new object[] { relicName, null };
                if((bool) Plugin.TryGetCustomRelicEffect.Invoke(null, args) && args[1] is RelicEffect)
                {
                    relic = relics.Find(r => r.effect == (RelicEffect) args[1]);
                    if (relic != null) return true;
                }
            }

            // Final attempt by searching internal names
            relic = relics.Find(r => r.locKey.Equals(relicName, StringComparison.OrdinalIgnoreCase));
            if (relic != null) return true;

            return false;
        }

        public static List<Relic> TryGetRelics(this RelicManager relicManager, String[] relicNames)
        {
            List<Relic> relics = new List<Relic>();
            foreach(String relicName in relicNames)
            {
                if(relicManager.TryGetRelic(relicName, out Relic relic))
                {
                    relics.Add(relic);
                }
            }

            return relics;
        }

        public static bool TryRemoveRelic(this RelicManager relicManager, String relicName)
        {
            if(relicManager.TryGetRelic(relicName, out Relic relic))
            {
                TryRemoveRelic(relicManager, relic);
                return true;
            }

            return false;
        }

        public static void TryRemoveRelic(this RelicManager relicManager, Relic relic)
        {
            relicManager._availableCommonRelics.Remove(relic);
            relicManager._availableRareRelics.Remove(relic);
            relicManager._availableBossRelics.Remove(relic);

            relicManager.CommonRelicPool.Remove(relic);
            relicManager.RareRelicPool.Remove(relic);
            relicManager.RareScenarioRelicPool.Remove(relic);
            relicManager.BossRelicPool.Remove(relic);
        }

        public static void TryRemoveRelics(this RelicManager relicManager, String[] relicNames)
        {
            foreach (String relicName in relicNames)
            {
                relicManager.TryRemoveRelic(relicName);
            }
        }
    }
}
