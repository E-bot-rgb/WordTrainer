using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordTrainer.Services
{
    public class WordValidator
    {
        private HashSet<string> _dictionary = new();
        private List<string> _wordList = new(); // För slumpmässigt val
        private readonly string WORDS_FILE;
        private Random _random = new Random();

        public WordValidator()
        {
            // Hitta projektets root från där exe:n körs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));

            WORDS_FILE = Path.Combine(projectRoot, "Data", "Words.txt");

            LoadDictionary();
        }

        private void LoadDictionary()
        {
            Console.WriteLine($"Söker efter: {WORDS_FILE}");
            Console.WriteLine($"Finns? {File.Exists(WORDS_FILE)}");

            if (File.Exists(WORDS_FILE))
            {
                foreach (var word in File.ReadLines(WORDS_FILE))
                {
                    string cleanWord = word.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(cleanWord))
                    {
                        _dictionary.Add(cleanWord);
                        _wordList.Add(cleanWord); // Behåll även i lista för random access
                    }
                }
                Console.WriteLine($"✅ Laddade {_dictionary.Count} ord");
            }
            else
            {
                Console.WriteLine("❌ Hittar inte ordlistan!");
            }
        }

        public bool IsValidWord(string word, string requiredLetters)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;

            word = word.Trim().ToLower();
            requiredLetters = requiredLetters.ToLower();

            if (!_dictionary.Contains(word)) return false;

            return word.Contains(requiredLetters);
        }

        public int GetWordCount()
        {
            return _dictionary.Count;
        }

        /// <summary>
        /// Hämta N slumpmässiga ord från ordlistan
        /// </summary>
        public List<string> GetRandomWords(int count)
        {
            if (_wordList.Count == 0) return new List<string>();

            var result = new List<string>();
            for (int i = 0; i < Math.Min(count, _wordList.Count); i++)
            {
                result.Add(_wordList[_random.Next(_wordList.Count)]);
            }

            return result;
        }

        /// <summary>
        /// Hämta ett slumpmässigt ord med minst X bokstäver
        /// </summary>
        public string GetRandomWord(int minLength = 3)
        {
            var validWords = _wordList.Where(w => w.Length >= minLength).ToList();

            if (validWords.Count == 0) return string.Empty;

            return validWords[_random.Next(validWords.Count)];
        }
    }
}