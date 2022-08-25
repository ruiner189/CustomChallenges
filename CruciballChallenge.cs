using System.Collections.Generic;

namespace CustomChallenges
{
    public class CruciballChallenge : Challenge
    {
        protected CruciballChallenge(Challenge challenge) : base(challenge.Id, challenge._data) {}

        public static CruciballChallenge From(Challenge challenge, params DataObject[] datas)
        {
            CruciballChallenge sub = new CruciballChallenge(challenge);

            if(datas != null)
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

        public static CruciballChallenge GetCruciballChallenge(Challenge original, int cruciballLevel)
        {
            if (original == null) return null;

            if (original.TryGetEntry<DataObject>(Properties.CRUCIBALL, out DataObject cruciball))
            {
                Plugin.Log.LogMessage("Found Cruciball");
                if (cruciball.TryGetEntry<DataObject>(Properties.LEVELS, out DataObject cruciballLevels))
                {
                    Plugin.Log.LogMessage("Found Levels");
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
                        return From(original, previousLevels.ToArray());
                    }
                    else if (cruciballLevels.TryGetEntry<DataObject>(cruciballLevel.ToString(), out DataObject level))
                    {
                        From(original, level);
                    }

                }
            }
            return From(original, null);
        }
    }
}
