namespace WordTrainer.Models
{
    public class Round
    {
        public string LetterCombination { get; set; }
        public int TimeLimit { get; set; }
        public Player CurrentPlayer { get; set; }
        public DateTime StartTime { get; set; }

        public Round(string letters, int timeLimit, Player player)
        {
            LetterCombination = letters;
            TimeLimit = timeLimit;
            CurrentPlayer = player;
            StartTime = DateTime.Now;
        }

        public int RemainingTime()
        {
            var elapsed = (DateTime.Now - StartTime).TotalSeconds;
            return Math.Max(0, TimeLimit - (int)elapsed);
        }

        public bool IsTimeUp()
        {
            return RemainingTime() <= 0;
        }
    }
}