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

namespace CustomChallenges
{
    [SceneModifier]
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance;
        public static Challenge CurrentChallenge;
        public static bool ChallengeActive => CurrentChallenge != null;

        public static readonly List<string> LoadedMods = new List<string>();

        private static JObject _challengeVictoryData;

        public void Awake()
        {
            if (Instance == null) Instance = this;
            if (Instance != this) Destroy(this);
        }

        public void Start()
        {
            GetActiveMods();
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
                    Plugin.Log.LogMessage($"{file}: {challenges.Count}");
                }
            }

            // Search all plugin folders
            path = Paths.PluginPath;
            foreach (String file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            {
                if (!file.EndsWith("manifest.json") && File.Exists(file))
                {
                    Plugin.Log.LogMessage(file);
                    Challenge.LoadChallenges(File.ReadAllText(file));
                }
            }
        }

        public static void OnSceneLoaded(String sceneName, bool firstLoad)
        {
            if (sceneName == SceneLoader.MainMenu)
            {
                Instance?.LoadMainMenuButton();
                Instance?.FixPlayButton();
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
            if (ChallengeActive)
            {
                String path = SaveFileManager.GetChallengeProgressSaveFile();
                _challengeVictoryData[CurrentChallenge.Id] = true;
                if(CurrentChallenge.TryGetEntry<bool>(Keys.ALLOW_CRUCIBALL, out bool allowCruciball) && allowCruciball)
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

        private GameObject challengePopup;
        private void LoadMainMenuButton()
        {
            GameObject buttonPrefab = GameObject.Find("OptionButton");
            GameObject challengeButton = GameObject.Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            challengeButton.name = "ChallengeButton";
            DestroyImmediate(challengeButton.GetComponent<UIButtonClickEventDispatcher>());
            DestroyImmediate(challengeButton.GetComponent<Button>());
            Localize localize = challengeButton.GetComponentInChildren<Localize>();
            localize?.SetTerm("Menu/CustomChallenges");
            challengeButton.AddComponent<Button>().onClick.AddListener(() => LoadChallengeMenu());
            challengeButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Challenges");
            challengeButton.transform.SetSiblingIndex(1);

            GameObject containerPrefab = GameObject.Find("CreditsContainer");
            GameObject challengeContainer = Instantiate(containerPrefab, containerPrefab.transform.parent);
            challengeContainer.name = "ChallengeContainer";

            challengePopup = challengeContainer.transform.GetChild(0).gameObject;
            challengePopup.name = "ChallengePopup";

            GameObject papyrus = challengePopup.transform.GetChild(2).gameObject; 
            GameObject scrollView = papyrus.transform.GetChild(0).gameObject;
            GameObject viewPort = scrollView.transform.GetChild(0).gameObject;
            GameObject buttonContainer = viewPort.transform.GetChild(0).gameObject;

            GameObject.Destroy(scrollView.GetComponent<AutoScroll>());
            foreach (Transform child in buttonContainer.transform)
            {
                Destroy(child.gameObject);
            }

            VerticalLayoutGroup vlg = buttonContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(0, 0, 25, 0);
            vlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject prefab = null;
            foreach(Challenge data in Challenge.GetSortedChallenges())
            {
                if(!Plugin.RevealAllChallenges && data.TryGetEntryArray<String>(Keys.REQUIRED_CHALLENGES, out String[] requiredChallenges))
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
                    ContentSizeFitter fitter = prefab.AddComponent<ContentSizeFitter>();
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
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

        private void LoadChallengeMenu()
        {
            challengePopup?.SetActive(true);
        }

        [HarmonyPatch(typeof(CruciballLevelSelector), nameof(CruciballLevelSelector.OnEnable))]
        public static class FixCruciballLevel
        {
            public static void Postfix(CruciballLevelSelector __instance)
            {
                __instance.ClassChanged(__instance.characterSelectController.currentlySelectedClass);
            }
        }
    }
}
