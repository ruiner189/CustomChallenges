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

        public readonly String Id;
        public String Name => GetEntry<String>(Properties.NAME);
        public bool IsNameLocalized => ContainsKey(Properties.LOCALIZATION_NAME);

        // CURRENTLY NOT IMPLEMENTED
        public String Description => GetEntry<String>(Properties.DESCRIPTION);
        public String Author => GetEntry<String>(Properties.AUTHOR);

        private static new Challenge From(JToken token)
        {
            return From(JObject.Load(token.CreateReader()));
        }

        private static Challenge From(JObject obj, bool include = true)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (JProperty property in obj.Properties())
            {
                data.Add(property.Name, GetValue(property.Value));
            }

            if (data.TryGetValue(Properties.ID, out object id) && id is string idString)
            {
                Challenge challenge = new Challenge(idString, data);
                if (challenge.TryGetEntryArray<String>(Properties.REQUIRED_MODS, out string[] requiredMods))
                    foreach (String mod in requiredMods)
                    {
                        if (!ChallengeManager.LoadedMods.Contains(mod))
                        {
                            Plugin.Log.LogMessage($"{challenge.Name} skipped due to missing the mod {mod}");
                            return null;
                        }
                    }

                if (challenge.TryGetEntry<DataObject>(Properties.LOCALIZATION_NAME, out DataObject localizedName))
                {
                    LoadLocalization($"Challenges/{challenge.Id}_name", localizedName);
                }

                if (challenge.TryGetEntry<DataObject>(Properties.LOCALIZATION_DESCRIPTION, out DataObject localizedDescription))
                {
                    LoadLocalization($"Challenges/{challenge.Id}_desc", localizedDescription);
                }

                if (include && challenge.IsChallengeValid())
                {
                    Plugin.Log.LogDebug($"Loaded Challenge: {challenge.Id}");
                    AllChallenges.Add(challenge);
                }
                return challenge;
            }

            return null;
        }

        public static Challenge Combine(Challenge challenge, params DataObject[] datas)
        {
            Challenge sub = new Challenge(challenge.Id, challenge._data);

            if (datas != null)
            {
                foreach (DataObject data in datas)
                {
                    foreach (var pair in data.GetData())
                    {
                        sub._data[pair.Key] = pair.Value;
                    }
                }
            }
            return sub;
        }

        public static Challenge GetCruciballChallenge(Challenge original, int cruciballLevel)
        {
            if (original == null) return null;

            if (original.TryGetEntry<DataObject>(Properties.CRUCIBALL, out DataObject cruciball))
            {
                if (cruciball.TryGetEntry<DataObject>(Properties.LEVELS, out DataObject cruciballLevels))
                {
                    if (cruciball.TryGetEntry<bool>(Properties.CASCADING_LEVELS, out bool cascading) && cascading)
                    {
                        List<DataObject> previousLevels = new List<DataObject>();
                        for (int i = 1; i < cruciballLevel + 1; i++)
                        {
                            if (cruciballLevels.TryGetEntry<DataObject>(i.ToString(), out DataObject level))
                            {
                                previousLevels.Add(level);
                            }
                        }
                        return Combine(original, previousLevels.ToArray());
                    }
                    else if (cruciballLevels.TryGetEntry<DataObject>(cruciballLevel.ToString(), out DataObject level))
                    {
                        Combine(original, level);
                    }

                }
            }
            return original;
        }

        private bool IsChallengeValid()
        {
            if(!ContainsKey(Properties.ID))
            {
                Plugin.Log.LogError($"Challenge invalid! Missing property {Properties.ID}");
                return false;
            }

            if(!ContainsKey(Properties.NAME))
            {
                Plugin.Log.LogError($"Challenge {Id} is invalid! Missing property {Properties.NAME}");
                return false;
            }

            if (AllChallenges.Find(challenge => challenge.Id == Id) != null)
            {
                Plugin.Log.LogError($"Duplicate challenge Id detected! Skipping {Id}");
                return false;
            }
            return true;
        }

        public bool IsCorrectVersion()
        {
            if(TryGetEntry<String>(Properties.VERSION, out String version))
            {
                if (version == Plugin.Version) return true;
                String[] challengeSplit = version.Split('.');
                int challengeMajor = int.Parse(challengeSplit[0]);
                int challengeMinor = int.Parse(challengeSplit[1]);
                int challengeIteration = int.Parse(challengeSplit[2]);

                String[] pluginSplit = Plugin.Version.Split('.');
                int pluginMajor = int.Parse(pluginSplit[0]);
                int pluginMinor = int.Parse(pluginSplit[1]);
                int pluginIteration = int.Parse(pluginSplit[2]);

                if (pluginMajor > challengeMajor) return true;
                else if (pluginMajor == challengeMajor)
                {
                    if (pluginMinor > challengeMinor) return true;
                    else if (pluginMinor == challengeMinor)
                    {
                        if (pluginIteration >= challengeIteration) return true;
                    }
                }

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

        protected Challenge(String id, Dictionary<string, object> data) {
            Id = id;
            foreach(KeyValuePair<string,object> pair in data)
            {
                _data.Add(pair.Key, pair.Value);
            }
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
                    if(localization.TryGetEntry<String>(Properties.SOURCE, out String url))
                    {
                        String path = null;
                        if(localization.TryGetEntry<String>(Properties.SOURCE_ID, out String localizationId)){
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

        public static Challenge LoadWeeklyChallenge(String json)
        {
            try
            {
                JObject jObject = JObject.Parse(json);
                Challenge challenge = From(jObject, false);
                return challenge;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e.StackTrace);
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            if(obj is Challenge challenge)
            {
                if (Id == challenge.Id) return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Challenge challengeA, Challenge challengeB)
        {
            // Pointer check
            if (ReferenceEquals(challengeA, challengeB)) return true;

            // Null checks
            if (challengeA is null) return false;
            if (challengeB is null) return false;

            return challengeA.Equals(challengeB);
        }

        public static bool operator !=(Challenge challengeA, Challenge challengeB)
        {
            return !(challengeA == challengeB);
        }
    }
}
