using Newtonsoft.Json;
using WordTrainer.Models;

namespace WordTrainer.Services
{
    /// <summary>
    /// Representerar en sparad match
    /// </summary>
    public class MatchData
    {
        public DateTime MatchDate { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int Score { get; set; }
        public int RoundsPlayed { get; set; }
        public int LivesRemaining { get; set; }
        public bool Won { get; set; }
        public List<string> WordsUsed { get; set; } = new();
        public TimeSpan TotalPlayTime { get; set; }
        public List<RoundPerformance> RoundPerformances { get; set; } = new(); // ← NYTT!
    }

    /// <summary>
    /// Prestanda för en enskild runda
    /// </summary>
    public class RoundPerformance
    {
        public string Letters { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? WordUsed { get; set; }
        public int TimeRemaining { get; set; }
    }

    /// <summary>
    /// Representerar en spelares kompletta profil
    /// </summary>
    public class PlayerProfile
    {
        public string PlayerName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastPlayed { get; set; }
        public int TotalMatches { get; set; }
        public int TotalWins { get; set; }
        public int TotalScore { get; set; }
        public int BestScore { get; set; }
        public List<MatchData> MatchHistory { get; set; } = new();

        // Statistik
        public int TotalWordsUsed => MatchHistory.Sum(m => m.WordsUsed.Count);
        public double WinRate => TotalMatches > 0 ? (double)TotalWins / TotalMatches * 100 : 0;
        public double AverageScore => TotalMatches > 0 ? (double)TotalScore / TotalMatches : 0;
    }

    /// <summary>
    /// Hanterar sparning och laddning av spelardata
    /// </summary>
    public class PlayerSaveManager
    {
        private readonly string _saveDirectory;

        public PlayerSaveManager()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
            _saveDirectory = Path.Combine(projectRoot, "Data", "Players");

            // Skapa mappen om den inte finns
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        /// <summary>
        /// Skapar ett säkert filnamn från spelarnamn
        /// </summary>
        private string GetSafeFileName(string playerName)
        {
            // Ta bort ogiltiga tecken
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                playerName = playerName.Replace(c, '_');
            }
            return playerName.ToLower().Trim() + ".json";
        }

        /// <summary>
        /// Hämta fullständig path till spelarens fil
        /// </summary>
        private string GetPlayerFilePath(string playerName)
        {
            return Path.Combine(_saveDirectory, GetSafeFileName(playerName));
        }

        /// <summary>
        /// Ladda en spelares profil (eller skapa ny)
        /// </summary>
        public PlayerProfile LoadOrCreateProfile(string playerName)
        {
            string filePath = GetPlayerFilePath(playerName);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var profile = JsonConvert.DeserializeObject<PlayerProfile>(json);

                    if (profile != null)
                    {
                        Console.WriteLine($"✅ Välkommen tillbaka, {profile.PlayerName}!");
                        Console.WriteLine($"   Tidigare matcher: {profile.TotalMatches}");
                        Console.WriteLine($"   Bästa poäng: {profile.BestScore}");
                        return profile;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Kunde inte ladda profil: {ex.Message}");
                }
            }

            // Skapa ny profil
            Console.WriteLine($"🆕 Ny spelare: {playerName}");
            return new PlayerProfile
            {
                PlayerName = playerName,
                CreatedDate = DateTime.Now,
                LastPlayed = DateTime.Now
            };
        }

        /// <summary>
        /// Spara matchdata för en spelare
        /// </summary>
        public void SaveMatch(Player player, GameSession session, DateTime matchStart, List<RoundPerformance> roundPerformances)
        {
            var profile = LoadOrCreateProfile(player.Name);

            // Skapa matchdata
            var matchData = new MatchData
            {
                MatchDate = matchStart,
                Difficulty = session.Difficulty,
                Score = player.Score,
                RoundsPlayed = session.RoundNumber - 1,
                LivesRemaining = player.Lives,
                Won = player.IsAlive && session.GetWinner() == player,
                WordsUsed = new List<string>(player.UsedWords),
                TotalPlayTime = DateTime.Now - matchStart,
                RoundPerformances = roundPerformances // ← NYTT!
            };

            // Uppdatera profil
            profile.MatchHistory.Add(matchData);
            profile.LastPlayed = DateTime.Now;
            profile.TotalMatches++;
            profile.TotalScore += player.Score;

            if (matchData.Won)
            {
                profile.TotalWins++;
            }

            if (player.Score > profile.BestScore)
            {
                profile.BestScore = player.Score;
            }

            // Spara till fil
            SaveProfile(profile);

            // Visa feedback
            Console.WriteLine($"\n💾 Match sparad för {player.Name}!");
            Console.WriteLine($"   Poäng denna match: {player.Score}");
            Console.WriteLine($"   Totalt matcher: {profile.TotalMatches}");
            Console.WriteLine($"   Win rate: {profile.WinRate:F1}%");
        }

        /// <summary>
        /// Spara profil till fil
        /// </summary>
        private void SaveProfile(PlayerProfile profile)
        {
            string filePath = GetPlayerFilePath(profile.PlayerName);
            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Visa spelarstatistik
        /// </summary>
        public void ShowPlayerStats(string playerName)
        {
            var profile = LoadOrCreateProfile(playerName);

            if (profile.TotalMatches == 0)
            {
                Console.WriteLine($"\n{playerName} har inga tidigare matcher.");
                return;
            }

            Console.WriteLine($"\n╔══════════════════════════════════════╗");
            Console.WriteLine($"║  📊 STATISTIK FÖR {profile.PlayerName.ToUpper().PadRight(18)} ║");
            Console.WriteLine($"╠══════════════════════════════════════╣");
            Console.WriteLine($"║  Totalt matcher:  {profile.TotalMatches.ToString().PadLeft(18)} ║");
            Console.WriteLine($"║  Vinster:         {profile.TotalWins.ToString().PadLeft(18)} ║");
            Console.WriteLine($"║  Win rate:        {profile.WinRate:F1}%".PadRight(40) + "║");
            Console.WriteLine($"║  Bästa poäng:     {profile.BestScore.ToString().PadLeft(18)} ║");
            Console.WriteLine($"║  Genomsnitt:      {profile.AverageScore:F0}".PadRight(40) + "║");
            Console.WriteLine($"║  Ord använda:     {profile.TotalWordsUsed.ToString().PadLeft(18)} ║");
            Console.WriteLine($"╚══════════════════════════════════════╝");

            // Visa senaste matcherna
            if (profile.MatchHistory.Count > 0)
            {
                Console.WriteLine($"\n🕐 SENASTE MATCHERNA:");
                var recentMatches = profile.MatchHistory
                    .OrderByDescending(m => m.MatchDate)
                    .Take(5);

                foreach (var match in recentMatches)
                {
                    string result = match.Won ? "🏆 VINST" : "💀 FÖRLUST";
                    Console.WriteLine($"  {match.MatchDate:yyyy-MM-dd HH:mm} | {result} | {match.Difficulty} | {match.Score}p | {match.RoundsPlayed} rundor");
                }
            }
        }

        /// <summary>
        /// Lista alla spelare
        /// </summary>
        public List<string> GetAllPlayers()
        {
            if (!Directory.Exists(_saveDirectory))
                return new List<string>();

            return Directory.GetFiles(_saveDirectory, "*.json")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToList();
        }

        /// <summary>
        /// Radera en spelares profil
        /// </summary>
        public bool DeletePlayer(string playerName)
        {
            string filePath = GetPlayerFilePath(playerName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"🗑️ Raderade profil för {playerName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Analysera bokstavsprestanda för en spelare
        /// </summary>
        public void ShowLetterAnalytics(string playerName)
        {
            var profile = LoadOrCreateProfile(playerName);

            if (profile.MatchHistory.Count == 0 || !profile.MatchHistory.Any(m => m.RoundPerformances.Count > 0))
            {
                Console.WriteLine($"\n[yellow]Inte tillräckligt med data för analys ännu. Spela mer![/yellow]");
                return;
            }

            // Samla all prestanda per bokstavskombination
            var letterStats = new Dictionary<string, (int successes, int failures, int totalTime)>();

            foreach (var match in profile.MatchHistory)
            {
                foreach (var round in match.RoundPerformances)
                {
                    if (!letterStats.ContainsKey(round.Letters))
                    {
                        letterStats[round.Letters] = (0, 0, 0);
                    }

                    var current = letterStats[round.Letters];
                    if (round.Success)
                    {
                        letterStats[round.Letters] = (current.successes + 1, current.failures, current.totalTime + round.TimeRemaining);
                    }
                    else
                    {
                        letterStats[round.Letters] = (current.successes, current.failures + 1, current.totalTime);
                    }
                }
            }

            // Beräkna success rate för varje kombination
            var letterPerformance = letterStats
                .Select(kvp => new
                {
                    Letters = kvp.Key,
                    Successes = kvp.Value.successes,
                    Failures = kvp.Value.failures,
                    Total = kvp.Value.successes + kvp.Value.failures,
                    SuccessRate = (double)kvp.Value.successes / (kvp.Value.successes + kvp.Value.failures) * 100,
                    AvgTimeRemaining = kvp.Value.successes > 0 ? (double)kvp.Value.totalTime / kvp.Value.successes : 0
                })
                .Where(x => x.Total >= 2) // Minst 2 försök för att vara relevant
                .ToList();

            if (letterPerformance.Count == 0)
            {
                Console.WriteLine($"\n[yellow]Inte tillräckligt med data för analys. Spela mer![/yellow]");
                return;
            }

            // Bästa bokstäver (högst success rate)
            var bestLetters = letterPerformance
                .OrderByDescending(x => x.SuccessRate)
                .ThenByDescending(x => x.AvgTimeRemaining)
                .Take(5)
                .ToList();

            // Sämsta bokstäver (lägst success rate)
            var worstLetters = letterPerformance
                .OrderBy(x => x.SuccessRate)
                .ThenBy(x => x.AvgTimeRemaining)
                .Take(5)
                .ToList();

            // Visa resultat
            Console.WriteLine($"\n╔══════════════════════════════════════════════╗");
            Console.WriteLine($"║  📊 BOKSTAVSANALYS FÖR {profile.PlayerName.ToUpper().PadRight(20)} ║");
            Console.WriteLine($"╠══════════════════════════════════════════════╣");

            Console.WriteLine($"║                                              ║");
            Console.WriteLine($"║  💚 BÄSTA BOKSTÄVER (Högst success rate):    ║");
            Console.WriteLine($"║                                              ║");

            foreach (var letter in bestLetters)
            {
                string bar = new string('█', (int)(letter.SuccessRate / 10));
                string display = $"  {letter.Letters.PadRight(4)} {letter.SuccessRate:F0}% {bar}".PadRight(44);
                Console.WriteLine($"║{display}║");
                Console.WriteLine($"║  └─ {letter.Successes}/{letter.Total} rätt, ⏱️ {letter.AvgTimeRemaining:F1}s kvar".PadRight(48) + "║");
            }

            Console.WriteLine($"║                                              ║");
            Console.WriteLine($"║  ❌ SÄMSTA BOKSTÄVER (Lägst success rate):   ║");
            Console.WriteLine($"║                                              ║");

            foreach (var letter in worstLetters)
            {
                string bar = new string('█', (int)(letter.SuccessRate / 10));
                string display = $"  {letter.Letters.PadRight(4)} {letter.SuccessRate:F0}% {bar}".PadRight(44);
                Console.WriteLine($"║{display}║");
                Console.WriteLine($"║  └─ {letter.Successes}/{letter.Total} rätt, ⏱️ {letter.AvgTimeRemaining:F1}s kvar".PadRight(48) + "║");
            }

            Console.WriteLine($"╚══════════════════════════════════════════════╝");

            // Rekommendationer
            Console.WriteLine($"\n💡 REKOMMENDATIONER:");

            if (worstLetters.Any())
            {
                var worstLetter = worstLetters.First();
                Console.WriteLine($"   • Träna mer på '{worstLetter.Letters}' - du har {worstLetter.SuccessRate:F0}% success rate");
            }

            if (bestLetters.Any())
            {
                var bestLetter = bestLetters.First();
                Console.WriteLine($"   • Du är bra på '{bestLetter.Letters}' - {bestLetter.SuccessRate:F0}% success rate!");
            }

            // Genomsnittlig success rate
            double overallSuccessRate = letterPerformance.Average(x => x.SuccessRate);
            Console.WriteLine($"   • Genomsnittlig success rate: {overallSuccessRate:F1}%");
        }
    }
}