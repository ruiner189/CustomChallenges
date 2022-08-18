using ProLib.Loaders;
using ProLib.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomChallenges
{
    public class Challenge : DataObject
    {
        public static readonly List<Challenge> AllChallenges = new List<Challenge>();

        public String Id => GetEntry<String>(Keys.ID);
        public String Name => GetEntry<String>(Keys.NAME);
        public bool IsNameLocalized => ContainsKey(Keys.LOCALIZATION_NAME);

        // CURRENTLY NOT IMPLEMENTED
        public String Description => GetEntry<String>(Keys.DESCRIPTION);
        public String Author => GetEntry<String>(Keys.AUTHOR);

        private static new Challenge From(JToken token)
        {
            Challenge challenge = new Challenge();
            JObject obj = JObject.Load(token.CreateReader());
            foreach (JProperty property in obj.Properties())
            {
                challenge._data.Add(property.Name, challenge.GetValue(property.Value));
            }

            if (challenge.TryGetEntryArray<String>(Keys.REQUIRED_MODS, out string[] requiredMods))
                foreach (String mod in requiredMods)
                {
                    if (!ChallengeManager.LoadedMods.Contains(mod))
                    {
                        Plugin.Log.LogMessage($"{challenge.Name} skipped due to missing the mod {mod}");
                        return null;
                    }
                }

            if(challenge.TryGetEntry<DataObject>(Keys.LOCALIZATION_NAME, out DataObject localizedName)){
                LoadLocalization($"Challenges/{challenge.Id}_name", localizedName);
            }

            if(challenge.TryGetEntry<DataObject>(Keys.LOCALIZATION_DESCRIPTION, out DataObject localizedDescription))
            {
                LoadLocalization($"Challenges/{challenge.Id}_desc", localizedDescription);
            }

            if (challenge.IsChallengeValid())
            {
                Plugin.Log.LogDebug($"Loaded Challenge: {challenge.Id}");
                AllChallenges.Add(challenge);
            }
            return challenge;
        }

        private bool IsChallengeValid()
        {
            if(!ContainsKey(Keys.ID))
            {
                Plugin.Log.LogError($"Challenge invalid! Missing property {Keys.ID}");
                return false;
            }

            if(!ContainsKey(Keys.NAME))
            {
                Plugin.Log.LogError($"Challenge {Id} is invalid! Missing property {Keys.NAME}");
                return false;
            }

            if (AllChallenges.Find(challenge => challenge.Id == Id) != null)
            {
                Plugin.Log.LogError($"Duplicate challenge Id detected! Skipping {Id}");
                return false;
            }
            return true;
        }

        private static void LoadLocalization(String key, DataObject localization)
        {
            LanguageLoader.Instance.LoadLocalizationSource(new Localization(
                key,
                localization.GetEntry<String>("English"),
                localization.GetEntry<String>("Français"),
                localization.GetEntry<String>("Español"),
                localization.GetEntry<String>("Deutsch"),
                localization.GetEntry<String>("Nederlands"),
                localization.GetEntry<String>("Italiano"),
                localization.GetEntry<String>("Português do Brasil"),
                localization.GetEntry<String>("Русский"),
                localization.GetEntry<String>("简体中文"),
                localization.GetEntry<String>("繁体中文"),
                localization.GetEntry<String>("日本語"),
                localization.GetEntry<String>("한국어"),
                localization.GetEntry<String>("Svenska"),
                localization.GetEntry<String>("Polski"),
                localization.GetEntry<String>("Türkçe")
                ));
        }

        public static List<Challenge> GetSortedChallenges()
        {
            return AllChallenges.OrderBy(challenge => challenge.Id).ToList();
        }

        protected Challenge() {}

        // CURRENTLY NOT IMPLEMENTED
        public CruciballChallenge GetCruciballChallenge(int cruciballLevel)
        {
            if(TryGetEntry<DataObject>(Keys.CRUCIBALL, out DataObject cruciball))
                if(cruciball.TryGetEntry<DataObject>(Keys.LEVELS, out DataObject cruciballLevels))
                    for(; ; cruciballLevel--)
                    {
                        if(cruciballLevels.TryGetEntry<DataObject>(cruciballLevel.ToString(), out DataObject level))
                        {
                            return CruciballChallenge.From(this, cruciball, level);
                        }
                    }
            return CruciballChallenge.From(this, null);
        }



        public static List<Challenge> LoadChallenges(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            return LoadChallenges(json);
        }

        public static List<Challenge> LoadChallenges(String json)
        {
            List<Challenge> challenges = new List<Challenge>();

            try
            {
                JObject jObject = JObject.Parse(json);

                if(jObject.TryGetValue("challenges", out JToken value))
                    foreach (JToken challenge in value)
                    {
                        Challenge data = From(challenge);
                        if (data != null)
                        {
                            challenges.Add(data);
                        }
                    }

                if(jObject.TryGetValue("localization", out JToken localizationToken))
                {
                    DataObject localization = DataObject.From(localizationToken);
                    if(localization.TryGetEntry<String>(Keys.GOOGLESHEET_LOCALIZATION, out String url))
                    {
                        String path = null;
                        if(localization.TryGetEntry<String>(Keys.LOCALIZATION_ID, out String localizationId)){
                            path = Path.Combine("Challenges", localizationId);
                        }
                        LanguageLoader.Instance.LoadGoogleSheetTSVSource(url, Path.Combine("Challenges", path));
                    }
                }
            }
            catch (Exception e) {
                Plugin.Log.LogError(e.StackTrace);
            }

            return challenges;
        }
    }
}
