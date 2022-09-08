using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using HarmonyLib;
using System.IO;
using PeglinUI.MainMenu;
using I2.Loc;
using PeglinUI.MainMenu.Cruciball;
using BepInEx.Bootstrap;
using Newtonsoft.Json.Linq;
using Cruciball;
using ProLib.Loaders;
using ProLib.Attributes;
using BepInEx;
using ProLib.Extensions;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using Peglin.ClassSystem;
using Saving;

namespace CustomChallenges
{
    [SceneModifier]
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance;
        public static Challenge CurrentChallenge;
        public static Challenge WeeklyChallenge;
        public static bool ChallengeActive => CurrentChallenge != null;

        public static readonly List<string> LoadedMods = new List<string>();

        private static JObject _challengeVictoryData;

        private int _currentCruciballLevel;
        public static int CurrentCruciballLevel => Instance._currentCruciballLevel;


        public void Awake()
        {
            if (Instance == null) Instance = this;
            if (Instance != this) Destroy(this);
        }

        public void Start()
        {
            GetActiveMods();
            StartCoroutine(LoadWeeklyChallenge());
            LoadChallengeVictoryData();
            LoadAllPluginChallenges();
            LoadAllLocalChallenges();
        }

        private void GetActiveMods()
        {
            foreach (var plugin in Chainloader.PluginInfos.Values)
            {
                LoadedMods.Add(plugin.Metadata.GUID);
            }
        }

        private void LoadAllPluginChallenges()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(!assembly.IsDynamic)
                    foreach(String path in assembly.GetManifestResourceNames())
                    {
                        if(path.Contains(".Resources.Challenges.") && path.EndsWith(".json")){
                            Challenge.LoadChallenges(assembly.GetManifestResourceStream(path));
                        }
                    }
            }
        }

        private void LoadAllLocalChallenges()
        {
            // Search in our generated folder in the save folder
            String path = SaveFileManager.GetChallengeDirectory();
            foreach(String file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            {
                if (File.Exists(file))
                {
                    List<Challenge> challenges = Challenge.LoadChallenges(File.ReadAllText(file));
                }
            }

            // Search all plugin folders
            path = Paths.PluginPath;
            foreach (String file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            {
                if (!file.EndsWith("manifest.json") && File.Exists(file))
                {
                    Challenge.LoadChallenges(File.ReadAllText(file));
                }
            }
        }

        private IEnumerator LoadWeeklyChallenge()
        {
            String url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vT8lGnrC2VndLG58lMxGqJBiOcBkGtW2ldTdfLTsg0vHpT_o0Wddr68PIJxUqKeH7ehF9rMrT4WWVmq/pub?gid=1598225879&single=true&output=tsv";
            String local = GetLocalWeeklyChallenge();

            if (local != null) WeeklyChallenge = Challenge.LoadWeeklyChallenge(local);

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Plugin.Log.LogWarning($"Could not fetch weekly challenge.");
                www.Dispose();
                yield break;
            }

            String online = www.downloadHandler.text;
            www.Dispose();

            if (local != online && online != null)
            {
                Challenge newWeekly = Challenge.LoadWeeklyChallenge(online);
                if(newWeekly != null)
                {
                    if (!newWeekly.IsCorrectVersion())
                    {
                        Plugin.Log.LogWarning("New weekly challenge uses a new version of the mod! Please update to play it.");
                        yield break;
                    }
                    Plugin.Log.LogInfo("Weekly Challenge has been updated!");
                    SaveWeeklyChallenge(online);
                    Challenge old = WeeklyChallenge;
                    WeeklyChallenge = newWeekly;
                    WeeklyChallengeButton?.SetChallengeData(WeeklyChallenge);
                    WeeklyChallengeButton?.gameObject.SetActive(true);
                    if (old != WeeklyChallenge && WeeklyChallenge != null)
                    {
                        // We are deleting the save file because it is a new weekly challenge.
                        SaveFileManager.DeleteSaveFile(WeeklyChallenge);
                    }

                } else
                {
                    Plugin.Log.LogError("There was a problem loading the new weekly challenge.");
                }
            }
        }

        private String GetLocalWeeklyChallenge()
        {
            String path = WeeklyChallengeFilePath();
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return null;
        }

        private void SaveWeeklyChallenge(String json)
        {
            String path = WeeklyChallengeFilePath();
            File.WriteAllText(path, json);
        }

        private String WeeklyChallengeFilePath()
        {
            return Path.Combine(SaveFileManager.GetModPath(), "WeeklyChallenge.json");
        }

        public static void OnSceneLoaded(String sceneName, bool firstLoad)
        {
            if (sceneName == SceneLoader.MainMenu)
            {
                Instance?.LoadMainMenuButton();
                Instance?.FixPlayButton();
            } else if (sceneName == SceneLoader.PostMainMenu)
            {
                Instance?.ApplyCruciball();
            } else if (sceneName == SceneLoader.FinalWinScene)
            {
                Instance?.SaveChallengeVictory();
            }
        }

        public static bool IsChallengeCompleted(String challengeID)
        {
            if(_challengeVictoryData != null)
            {
                if(_challengeVictoryData.TryGetValue(challengeID, out JToken result))
                {
                    return (bool) result;
                }
            }
            return false;
        }

        public static bool IsChallengeMaxCruciball(Challenge challenge)
        {
            if(_challengeVictoryData != null)
            {
                if(_challengeVictoryData.TryGetValue(challenge.Id + "Cruciball", out JToken result))
                {
                    if (((int)result) == 7) return true;
                }
            }
            return false;
        }


        private void SaveChallengeVictory()
        {
            if (ChallengeActive && CurrentChallenge != WeeklyChallenge)
            {
                String path = SaveFileManager.GetChallengeProgressSaveFile();
                _challengeVictoryData[CurrentChallenge.Id] = true;
                if(CurrentChallenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool allowCruciball) && allowCruciball)
                {
                    CruciballManager manager = Resources.FindObjectsOfTypeAll<CruciballManager>().FirstOrDefault();
                    if(manager != null)
                    {
                        String key = CurrentChallenge.Id + "Cruciball";
                        int currentLevel = manager.currentCruciballLevel;
                        if (!_challengeVictoryData.ContainsKey(key) || (int)_challengeVictoryData[key] <= currentLevel)
                            _challengeVictoryData[key] = currentLevel;
                    }
                }
                File.WriteAllText(path, _challengeVictoryData.ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }

        private void LoadChallengeVictoryData()
        {
            String path = SaveFileManager.GetChallengeProgressSaveFile();
            if (File.Exists(path))
            {
                _challengeVictoryData = JObject.Parse(File.ReadAllText(path));
            } else
            {
                _challengeVictoryData = new JObject();
            }
        }

        private void ApplyCruciball()
        {
            CruciballManager cruciballManager = Resources.FindObjectsOfTypeAll<CruciballManager>().FirstOrDefault();
            _currentCruciballLevel = cruciballManager.currentCruciballLevel;

            if (ChallengeActive)
            {
                if(CurrentChallenge.TryGetEntry<DataObject>(Properties.CRUCIBALL, out DataObject cruciball))
                {
                    if (cruciball.TryGetEntry<bool>(Properties.OVERWRITE_CRUCIBALL_LEVELS, out bool overwriteCruciball) && overwriteCruciball)
                    {
                        if(cruciballManager != null)
                        {
                            cruciballManager.currentCruciballLevel = -1;
                        }
                    }
                }
            }
        }

        private void FixPlayButton()
        {
            GameObject gameObject = GameObject.Find("PlayButton");
            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => SetToDefaultSave());
            button.onClick.AddListener(() => ResetDescriptions());
        }

        private void SetToDefaultSave()
        {
            GameObject button = GameObject.Find("PlayButton");
            PlayButton playButton = button.GetComponent<PlayButton>();
            CurrentChallenge = null;
            SaveFileManager.ReloadSaveFile();
            playButton._mapControllerSaveData = null;
            playButton.OnEnable();

            if (ChallengeButton.CustomButton != null)
                ChallengeButton.CustomButton.SetActive(true);
        }

        private void ResetDescriptions()
        {
            GameObject panel = Camera.main.gameObject.FindChild(
                "Character+CruciballCanvas",
                "ClassDetailsPanel"
                );
            SetTitleText(panel.FindChild("ClassTitle"));
            SetDescriptionText(panel.FindChild("ClassDescription"));
            SetFooterText(panel.FindChild("MoreClassesSoon!"));
        }

        public void SetTitleText(GameObject title)
        {
            Localize localize = title.GetComponent<Localize>();
            localize.SetTerm("Classes/peglin_class_title");
            localize.enabled = true;
        }

        public void SetDescriptionText(GameObject description)
        {
            Localize localize = description.GetComponent<Localize>();
            localize.SetTerm("Classes/peglin_class_desc");
            localize.enabled = true;
        }

        public void SetFooterText(GameObject footer)
        {
            Localize localize = footer.GetComponent<Localize>();
            TextMeshProUGUI text = footer.GetComponent<TextMeshProUGUI>();
            localize.SetTerm("Classes/more_classes_coming_soon");
            text.enabled = true;
            localize.enabled = true;
        }

        private GameObject ChallengePopup;
        private WeeklyChallengeButton WeeklyChallengeButton;
        private void LoadMainMenuButton()
        {
            GameObject weeklyChallengeButtonObject = CreateMenuButton("WeeklyChallengeButton", "Menu/WeeklyChallenge", "Weekly Challenge", 1);
            weeklyChallengeButtonObject.AddComponent<Button>();
            WeeklyChallengeButton = weeklyChallengeButtonObject.AddComponent<WeeklyChallengeButton>();
            WeeklyChallengeButton.SetChallengeData(WeeklyChallenge);

            if (WeeklyChallenge == null) weeklyChallengeButtonObject.SetActive(false);

            GameObject container = GameObject.Find("PeglinLogo");
            container.transform.position += new Vector3(0, 1, 0);
            GameObject challengeButton = CreateMenuButton("ChallengeButton", "Menu/CustomChallenges", "Challenges", 2);
            challengeButton.AddComponent<Button>().onClick.AddListener(() => LoadChallengeMenu());

            GameObject containerPrefab = GameObject.Find("CreditsContainer");
            GameObject challengeContainer = Instantiate(containerPrefab, containerPrefab.transform.parent);
            challengeContainer.name = "ChallengeContainer";

            ChallengePopup = challengeContainer.transform.GetChild(0).gameObject;
            ChallengePopup.name = "ChallengePopup";

            GameObject papyrus = ChallengePopup.transform.GetChild(2).gameObject; 
            GameObject scrollView = papyrus.transform.GetChild(0).gameObject;
            GameObject viewPort = scrollView.transform.GetChild(0).gameObject;
            GameObject buttonContainer = viewPort.transform.GetChild(0).gameObject;

            GameObject.Destroy(scrollView.GetComponent<AutoScroll>());
            foreach (Transform child in buttonContainer.transform)
            {
                Destroy(child.gameObject);
            }

            DestroyImmediate(buttonContainer.GetComponent<VerticalLayoutGroup>());

            GridLayoutGroup glg = buttonContainer.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 25);
            glg.padding = new RectOffset(40, 0, 10, 0);
            glg.childAlignment = TextAnchor.UpperLeft;

            GameObject prefab = null;
            foreach(Challenge data in Challenge.GetSortedChallenges())
            {
                if(!Plugin.RevealAllChallenges && data.TryGetEntryArray<String>(Properties.REQUIRED_CHALLENGES, out String[] requiredChallenges))
                {
                    bool shouldSkip = false;
                    foreach(String challengeID in requiredChallenges)
                    {
                        if (!IsChallengeCompleted(challengeID))
                        {
                            shouldSkip = true;
                            break;
                        }
                    }
                    if (shouldSkip)
                        continue;
                }

                if(prefab == null)
                {
                    prefab = new GameObject("ChallengeButton");
                    prefab.AddComponent<LayoutElement>();

                    ButtonHandleHover hover = prefab.AddComponent<ButtonHandleHover>();
                    hover._origTextColor = Color.white;
                    hover.swapColor = Color.green;
                    Button b = prefab.AddComponent<Button>();
                    ChallengeButton cb = prefab.AddComponent<ChallengeButton>();
                    cb.SetChallengeData(data);

                    GameObject text = new GameObject("text");
                    text.transform.parent = prefab.transform;
                    TextMeshProUGUI textMesh = text.AddComponent<TextMeshProUGUI>();

                    textMesh.fontSize = 24;
                    textMesh.verticalAlignment = VerticalAlignmentOptions.Capline;
                    prefab.transform.SetParent(buttonContainer.transform);
                    prefab.transform.localPosition = new Vector3(0, 0, 0);
                    prefab.transform.localScale = new Vector3(1, 1, 1);
                } 
                else
                {
                    GameObject button = Instantiate(prefab, prefab.transform.parent);
                    button.GetComponent<ChallengeButton>().SetChallengeData(data);
                }
            }
        }

        private GameObject CreateMenuButton(String name, String localizeTerm, String defaultText, int index)
        {
            GameObject buttonPrefab = GameObject.Find("OptionButton");
            GameObject button = GameObject.Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            button.name = name;
            DestroyImmediate(button.GetComponent<UIButtonClickEventDispatcher>());
            DestroyImmediate(button.GetComponent<Button>());
            button.GetComponentInChildren<Localize>()?.SetTerm(localizeTerm);
            button.GetComponentInChildren<TextMeshProUGUI>().SetText(defaultText);
            button.transform.SetSiblingIndex(index);
            return button;
        }

        private void LoadChallengeMenu()
        {
            ChallengePopup?.SetActive(true);
        }

        [HarmonyPatch(typeof(CruciballLevelSelector), nameof(CruciballLevelSelector.OnEnable))]
        public static class FixCruciballLevelSelection
        {
            public static void Postfix(CruciballLevelSelector __instance)
            {
                __instance.ClassChanged(__instance.characterSelectController.currentlySelectedClass);
            }
        }

        [HarmonyPatch(typeof(CruciballLevelSelector), nameof(CruciballLevelSelector.ClassChanged))]
        public static class ApplyMinLevel
        {
            public static void Prefix(Class newClass)
            {
                if (ChallengeActive)
                {
                    if (CurrentChallenge.TryGetEntry<DataObject>(Properties.CRUCIBALL, out DataObject cruciball))
                    {
                        if (cruciball.TryGetEntry<int>(Properties.STARTING_CRUCIBALL_LEVEL, out int startingLevel))
                        {
                            if (PersistentPlayerData.Instance.CruciballLevels[newClass] < startingLevel)
                            {
                                PersistentPlayerData.Instance.CruciballLevels[newClass] = startingLevel;
                            }
                        }
                    }
                }

            }
        }


        [HarmonyPatch(typeof(BattleController), nameof(BattleController.CompleteVictory))]
        public static class FixCruciballLevel
        {
            public static void Prefix(BattleController __instance)
            {
                if (ChallengeActive)
                {
                    if(CurrentChallenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool allowCruciball) && allowCruciball)
                    {
                        if(StaticGameData.currentNode != null && StaticGameData.currentNode.isFinalNode && StaticGameData.activeMapScene == SceneLoader.MinesMap)
                        {
                            __instance._cruciballManager.currentCruciballLevel = CurrentCruciballLevel;
                        }
                    }
                }
            }
        }
    }
}
