using Battle.Enemies;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Cruciball;
using CustomChallenges.UI;
using HarmonyLib;
using I2.Loc;
using ProLib.Attributes;
using ProLib.Extensions;
using ProLib.Loaders;
using ProLib.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;

namespace CustomChallenges
{

    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency("com.ruiner.prolib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.github.crazyjackel.RelicLib", BepInDependency.DependencyFlags.SoftDependency)]

    public class Plugin : BaseUnityPlugin
    {
        public const String GUID = "com.ruiner.customchallenges";
        public const String Name = "Custom Challenges";
        public const String Version = "1.2.1";

        private static ConfigEntry<bool> _revealAllChallenges;
        public static bool RevealAllChallenges => _revealAllChallenges.Value;


        private Harmony _harmony;
        public static ManualLogSource Log;
        public static ConfigFile ConfigFile;
        public static GameObject ChallengeManager;
        internal static MethodInfo TryGetCustomRelicEffect;

        private void Awake()
        {
            Log = Logger;
            LoadConfig();
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();
            ChallengeManager = new GameObject("Challenge Manager");
            ChallengeManager.AddComponent<ChallengeManager>();
            ChallengeManager.AddComponent<WinConditionManager>();
            DontDestroyOnLoad(ChallengeManager);
            ChallengeManager.hideFlags = HideFlags.HideAndDontSave;

            LoadSoftDependencies();
        }

        [Register]
        public static void Register()
        {
            LanguageLoader.Instance.LoadGoogleSheetTSVSource("https://docs.google.com/spreadsheets/d/e/2PACX-1vRe82XVSt8LOUz3XewvAHT5eDDzAqXr5MV0lt3gwvfN_2n9Zxj613jllVPtdPdQweAap2yOSJSgwpPt/pub?gid=382319925&single=true&output=tsv", "CustomChallenges_Translations.tsv");
        }

        private void LoadConfig()
        {
            ConfigFile = Config;
            _revealAllChallenges = Config.Bind<bool>("General", "RevealAllChallenges", false, "Enable to reveal all challenges. Otherwise, you must have the prerequisites to see it.");
        }

        private void LoadSoftDependencies()
        {
            if (Chainloader.PluginInfos.TryGetValue("io.github.crazyjackel.RelicLib", out BepInEx.PluginInfo plugin))
            {
                Assembly assembly = plugin.Instance.GetType().Assembly;
                Type[] Types = AccessTools.GetTypesFromAssembly(assembly);
                Type register = Types.FirstOrDefault(x => x.Name == "RelicRegister");
                TryGetCustomRelicEffect = AccessTools.Method(register, "TryGetCustomRelicEffect");
            }
        }
    }
}

