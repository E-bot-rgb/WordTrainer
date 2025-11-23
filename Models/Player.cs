namespace WordTrainer.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int Lives { get; set; }
        public int Score { get; set; }
        public bool IsAlive => Lives > 0;
        public List<string> UsedWords { get; set; } = new();

        public Player(string name, int lives = 3)
        {
            Name = name;
            Lives = lives;
            Score = 0;
        }

        public void LoseLife()
        {
            if (Lives > 0) Lives--;
        }

        public void AddScore(int points)
        {
            Score += points;
        }

        public bool HasUsedWord(string word)
        {
            return UsedWords.Contains(word.ToLower());
        }

        public void AddUsedWord(string word)
        {
            UsedWords.Add(word.ToLower());
        }
    }
}