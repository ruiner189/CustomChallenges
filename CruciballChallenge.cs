namespace CustomChallenges
{
    public class CruciballChallenge : Challenge
    {
        public static CruciballChallenge From(Challenge challenge, params DataObject[] datas)
        {
            CruciballChallenge sub = new CruciballChallenge();
            foreach(var pair in challenge.GetData())
            {
                if (pair.Key == Keys.CRUCIBALL)
                    continue;
                sub._data[pair.Key] = pair.Value;
            }

            foreach(DataObject data in datas)
            {
                foreach (var pair in data.GetData())
                {
                    sub._data[pair.Key] = pair.Value;
                }
            }
            return sub;
        } 
    }
}
