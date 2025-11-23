using System.IO;

namespace WordTrainer.Services
{
    public class WordValidator
    {
        private HashSet<string> _dictionary = new();
        private const string WORDS_FILE = "Data/Words.txt";

        public WordValidator()
        {
            LoadDictionary();
        }

        private void LoadDictionary()
        {
            if (File.Exists(WORDS_FILE))
            {
                foreach (var word in File.ReadLines(WORDS_FILE))
                {
                    _dictionary.Add(word.Trim().ToLower());
                }
            }
        }

        public bool IsValidWord(string word, string requiredLetters)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;

            word = word.Trim().ToLower();
            requiredLetters = requiredLetters.ToLower();

            // Kontrollera att ordet finns i ordlistan
            if (!_dictionary.Contains(word)) return false;

            // Kontrollera att bokstavskombinationen finns i ordet
            return word.Contains(requiredLetters);
        }

        public int GetWordCount()
        {
            return _dictionary.Count;
        }
    }
}