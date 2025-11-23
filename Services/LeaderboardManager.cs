using Newtonsoft.Json;
using WordTrainer.Models;

namespace WordTrainer.Services
{
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public string Difficulty { get; set; }
        public DateTime Date { get; set; }
    }

    public class LeaderboardManager
    {
        private const string LEADERBOARD_FILE = "Data/leaderboard.json";
        private List<LeaderboardEntry> _entries = new();

        public LeaderboardManager()
        {
            LoadLeaderboard();
        }

        public void AddEntry(Player player, string difficulty)
        {
            _entries.Add(new LeaderboardEntry
            {
                PlayerName = player.Name,
                Score = player.Score,
                Difficulty = difficulty,
                Date = DateTime.Now
            });

            _entries = _entries.OrderByDescending(e => e.Score).Take(10).ToList();
            SaveLeaderboard();
        }

        public List<LeaderboardEntry> GetTopScores(int count = 10)
        {
            return _entries.Take(count).ToList();
        }

        private void LoadLeaderboard()
        {
            if (File.Exists(LEADERBOARD_FILE))
            {
                string json = File.ReadAllText(LEADERBOARD_FILE);
                _entries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json) ?? new();
            }
        }

        private void SaveLeaderboard()
        {
            string json = JsonConvert.SerializeObject(_entries, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(LEADERBOARD_FILE));
            File.WriteAllText(LEADERBOARD_FILE, json);
        }
    }
}