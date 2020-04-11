using WCell.Constants.Achievements;

namespace WCell.RealmServer.Asda2Titles
{
    public class Asda2TitleProgress
    {
        const int MaxCounterValue = 2000000000;
        public void InitNew()
        {
            ProgressRecords = new Asda2TitleProgressRecord[1000];
        }

        public int IncreaseCounter(Asda2TitleId title, int increaceCounterBy = 1)
        {
            if (ProgressRecords[(int)title] == null)
            {
                ProgressRecords[(int)title] = new Asda2TitleProgressRecord
                {
                    TitleId = (int)title,
                    Counter = 0
                };
            }
            else if (ProgressRecords[(int) title].Counter >= MaxCounterValue)
            {
                return ProgressRecords[(int) title].Counter;
            }

            ProgressRecords[(int)title].Counter+=increaceCounterBy;

            return ProgressRecords[(int) title].Counter;
        }



        public Asda2TitleProgressRecord[] ProgressRecords { get; set; }
    }

    public class Asda2TitleProgressRecord
    {
        public int TitleId { get; set; }
        public int Counter { get; set; }
    }
}