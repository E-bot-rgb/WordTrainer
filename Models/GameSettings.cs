namespace WordTrainer.Models
{
    public class GameSettings
    {
        public int EasyTimeLimit { get; set; } = 20;
        public int NormalTimeLimit { get; set; } = 15;
        public int HardTimeLimit { get; set; } = 10;
        public int StartingLives { get; set; } = 3;
        public int LetterCountEasy { get; set; } = 2;
        public int LetterCountNormal { get; set; } = 3;
        public int LetterCountHard { get; set; } = 3;

        public int GetTimeLimit(string difficulty)
        {
            return difficulty switch
            {
                "Easy" => EasyTimeLimit,
                "Normal" => NormalTimeLimit,
                "Hard" => HardTimeLimit,
                _ => NormalTimeLimit
            };
        }

        public int GetLetterCount(string difficulty)
        {
            return difficulty switch
            {
                "Easy" => LetterCountEasy,
                "Normal" => LetterCountNormal,
                "Hard" => LetterCountHard,
                _ => LetterCountNormal
            };
        }
    }
}