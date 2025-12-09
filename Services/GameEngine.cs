using WordTrainer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordTrainer.Services
{
    public class GameEngine
    {
        private WordValidator _validator;
        private GameSettings _settings;
        private Random _random = new Random(); // Viktigt: en instans för hela klassen
        private readonly string LETTERS_FILE;
        private List<string> _letterPool = new(); // Cache för bokstavskombinationer

        public GameEngine()
        {
            _validator = new WordValidator();
            _settings = new GameSettings();

            // Samma fix som tidigare för path
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
            LETTERS_FILE = Path.Combine(projectRoot, "Data", "found.txt");

            // Ladda bokstavskombinationer vid start
            LoadLetterPool();
        }

        /// <summary>
        /// Ladda alla möjliga bokstavskombinationer från fil
        /// </summary>
        private void LoadLetterPool()
        {
            if (File.Exists(LETTERS_FILE))
            {
                _letterPool = File.ReadAllLines(LETTERS_FILE)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim().ToUpper())
                    .ToList();

                Console.WriteLine($"✅ Laddade {_letterPool.Count} bokstavskombinationer");
            }
            else
            {
                Console.WriteLine($"⚠️ Hittar inte {LETTERS_FILE}, använder random generation");
            }
        }

        public Round StartNewRound(GameSession session)
        {
            string letters = GenerateLetters(session.Difficulty);
            int timeLimit = _settings.GetTimeLimit(session.Difficulty);

            return new Round(letters, timeLimit, session.CurrentPlayer);
        }

        public (bool success, string message) ValidateAnswer(string word, Round round)
        {
            // Kontrollera om tiden har gått ut
            if (round.IsTimeUp())
            {
                return (false, "⏱️ Tiden är ute!");
            }

            // Kontrollera om ordet redan använts
            if (round.CurrentPlayer.HasUsedWord(word))
            {
                return (false, "❌ Du har redan använt det ordet!");
            }

            // Kontrollera om ordet är giltigt
            if (!_validator.IsValidWord(word, round.LetterCombination))
            {
                return (false, $"❌ Ogiltigt ord! Måste innehålla '{round.LetterCombination}'");
            }

            return (true, "✅ Rätt svar!");
        }

        public int CalculateScore(Round round, string word)
        {
            int baseScore = 10;
            int timeBonus = round.RemainingTime(); // Extra poäng för snabbt svar
            int lengthBonus = word.Length; // Bonus för längre ord

            return baseScore + timeBonus + lengthBonus;
        }

        /// <summary>
        /// Generera slumpmässiga bokstäver baserat på svårighetsgrad
        /// </summary>
        private string GenerateLetters(string difficulty)
        {
            int letterCount = _settings.GetLetterCount(difficulty);

            // BÄSTA METODEN: Generera från riktiga ord i ordlistan
            // Detta garanterar att det alltid finns ord att hitta!
            string letters = GenerateFromDictionary(letterCount);

            if (!string.IsNullOrEmpty(letters))
            {
                return letters;
            }

            // Fallback 1: Använd loaded pool om tillgänglig
            if (_letterPool.Count > 0)
            {
                return GenerateFromPool(letterCount);
            }

            // Fallback 2: Generera helt slumpmässigt
            return GenerateRandomLetters(letterCount);
        }

        /// <summary>
        /// Generera från pool av ord (found.txt)
        /// </summary>
        private string GenerateFromPool(int letterCount)
        {
            // Filtrera kombinationer som är rätt längd
            var validCombos = _letterPool.Where(combo => combo.Length >= letterCount).ToList();

            if (validCombos.Count == 0)
            {
                return GenerateRandomLetters(letterCount);
            }

            // Välj ett slumpmässigt ord
            string selectedWord = validCombos[_random.Next(validCombos.Count)];

            // Ta ut en slumpmässig sekvens
            if (selectedWord.Length == letterCount)
            {
                return selectedWord;
            }
            else
            {
                int startIndex = _random.Next(0, selectedWord.Length - letterCount + 1);
                return selectedWord.Substring(startIndex, letterCount);
            }
        }

        /// <summary>
        /// Generera helt slumpmässiga bokstäver
        /// </summary>
        private string GenerateRandomLetters(int count)
        {
            // Vanliga bokstavskombinationer på engelska
            const string COMMON_CONSONANTS = "TNSRHDLCMFPGWYBVKJXQZ";
            const string COMMON_VOWELS = "EAIOUY";

            string result = "";

            for (int i = 0; i < count; i++)
            {
                // Blanda vokaler och konsonanter för mer naturliga kombinationer
                if (i % 2 == 0 || _random.Next(100) < 30)
                {
                    // Vokal
                    result += COMMON_VOWELS[_random.Next(COMMON_VOWELS.Length)];
                }
                else
                {
                    // Konsonant
                    result += COMMON_CONSONANTS[_random.Next(COMMON_CONSONANTS.Length)];
                }
            }

            return result;
        }

        /// <summary>
        /// Generera bokstäver från faktiska ord i ordlistan (BÄSTA METODEN)
        /// </summary>
        public string GenerateFromDictionary(int letterCount)
        {
            // Hämta ett slumpmässigt ord från validator
            var words = _validator.GetRandomWords(100); // Få 100 slumpmässiga ord

            if (words.Count == 0)
            {
                return GenerateRandomLetters(letterCount);
            }

            // Välj ett ord som är tillräckligt långt
            var validWords = words.Where(w => w.Length >= letterCount).ToList();

            if (validWords.Count == 0)
            {
                return GenerateRandomLetters(letterCount);
            }

            string word = validWords[_random.Next(validWords.Count)];
            int startPos = _random.Next(0, word.Length - letterCount + 1);

            return word.Substring(startPos, letterCount).ToUpper();
        }
    }
}