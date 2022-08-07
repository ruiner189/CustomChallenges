using ProLib.Attributes;
using ProLib.Loaders;
using HarmonyLib;
using PeglinUI.MainMenu;
using Saving;
using System;
using System.IO;
using ToolBox.Serialization;
using UnityEngine;
using Cruciball;
using Battle;

namespace CustomChallenges
{
    [SceneModifier]
    public static class SaveFileManager
    {
        public static String GetModPath()
        {
            return Path.Combine(Application.persistentDataPath, Plugin.Name);
        }

        [Register]
        public static void CreateSaveDirectories()
        {
            Directory.CreateDirectory(GetSaveDirectory());
            Directory.CreateDirectory(GetChallengeDirectory());
        }

        public static String GetChallengeDirectory()
        {
            return Path.Combine(GetModPath(), "challenges");
        }

        public static String GetSaveDirectory()
        {
            return Path.Combine(GetModPath(), "saves");
        }

        public static String GetSaveFile(String challengeID)
        {
            return Path.Combine(GetModPath(), "saves", string.Format("{0}_{1}.data", "Save", challengeID));
        }

        public static String GetSaveFile(Challenge challenge)
        {
            return GetSaveFile(challenge.Id);
        }

        public static String GetChallengeProgressSaveFile()
        {
            return Path.Combine(GetSaveDirectory(), "save.json");
        }

        public static String GetDefaultSavePath()
        {
           return Path.Combine(Application.persistentDataPath, string.Format("{0}_{1}.data", "Save", DataSerializer._currentProfileIndex));
        }

        public static void ReloadSaveFile()
        {
            PersistentPlayerData._instance = null;
            DataSerializer.LoadFile();
            SaveManager.RequestLoad();
        }

        public static void OnLateSceneLoaded(String sceneName, bool firstLoad)
        {
            if (!firstLoad && sceneName == SceneLoader.MainMenu && ChallengeManager.ChallengeActive)
            {
                ChallengeManager.CurrentChallenge = null;
                GameObject button = GameObject.Find("PlayButton");
                if(button != null)
                {
                    PlayButton playButton = button.GetComponent<PlayButton>();
                    playButton._mapControllerSaveData = null;

                    SaveFileManager.ReloadSaveFile();

                    playButton.OnEnable();
                }
            }
        }


        [HarmonyPatch(typeof(DataSerializer), nameof(DataSerializer.GetFilePath))]
        public static class ChangeSavePath
        {
            public static bool Prefix(ref String __result)
            {
                if (!ChallengeManager.ChallengeActive) return true;
                __result = GetSaveFile(ChallengeManager.CurrentChallenge);
                return false;
            }
        }

        [HarmonyPatch(typeof(CruciballManager), nameof(CruciballManager.CruciballVictoryAchieved))]
        private class SaveCruciballData
        {
            public static void Postfix(SaveManager __instance)
            {
                __instance.SaveFile();
            }
        }

        [HarmonyPatch(typeof(PlayerHealthController), nameof(PlayerHealthController.CheckForDeathAndUpdateBar))]
        private class SaveAfterDying
        {
            public static void Postfix(PlayerHealthController __instance)
            {
                if(__instance._playerHealth.Value <= 0)
                {
                    SaveManager.Instance.SaveFile();
                }
            }
        }
    }
}
