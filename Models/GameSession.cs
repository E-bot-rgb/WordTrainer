namespace WordTrainer.Models
{
    public class GameSession
    {
        public List<Player> Players { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;
        public int RoundNumber { get; set; } = 1;
        public string Difficulty { get; set; }

        public Player CurrentPlayer => Players[CurrentPlayerIndex];

        public GameSession(List<Player> players, string difficulty)
        {
            Players = players;
            Difficulty = difficulty;
        }

        public void NextPlayer()
        {
            do
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            } while (!CurrentPlayer.IsAlive && GetAlivePlayers().Count > 1);
        }

        public List<Player> GetAlivePlayers()
        {
            return Players.Where(p => p.IsAlive).ToList();
        }

        public Player? GetWinner()
        {
            var alive = GetAlivePlayers();
            return alive.Count == 1 ? alive[0] : null;
        }
    }
}