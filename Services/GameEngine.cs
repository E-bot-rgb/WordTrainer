using WordTrainer.Models;
using System;

namespace WordTrainer.Services
{
    public class GameEngine
    {
        private WordValidator _validator;
        private GameSettings _settings;
        private Random _random = new();
        private const string LETTERS_FILE = "Data/found.txt";

        public GameEngine()
        {
            _validator = new WordValidator();
            _settings = new GameSettings();
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

        private string GenerateLetters(string difficulty)
        {
            if (!File.Exists(LETTERS_FILE))
            {
                // Fallback: generera slumpmässiga bokstäver
                return GenerateRandomLetters(difficulty);
            }

            var words = File.ReadAllLines(LETTERS_FILE);
            if (words.Length == 0) return GenerateRandomLetters(difficulty);

            string selectedWord = words[_random.Next(words.Length)];
            int letterCount = _settings.GetLetterCount(difficulty);

            // Ta ut en slumpmässig sekvens från ordet
            if (selectedWord.Length >= letterCount)
            {
                int startIndex = _random.Next(0, selectedWord.Length - letterCount + 1);
                return selectedWord.Substring(startIndex, letterCount).ToUpper();
            }

            return GenerateRandomLetters(difficulty);
        }

        private string GenerateRandomLetters(string difficulty)
        {
            const string vowels = "AEIOUY";
            const string consonants = "BCDFGHJKLMNPQRSTVWXZ";
            int count = _settings.GetLetterCount(difficulty);

            string result = "";
            for (int i = 0; i < count; i++)
            {
                // Blanda vokaler och konsonanter
                string pool = (i % 2 == 0) ? consonants : vowels;
                result += pool[_random.Next(pool.Length)];
            }

            return result;
        }
    }
}