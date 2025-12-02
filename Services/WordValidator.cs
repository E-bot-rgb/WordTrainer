using System.IO;

namespace WordTrainer.Services
{
    public class WordValidator
    {
        private HashSet<string> _dictionary = new();
        private readonly string WORDS_FILE;

        public WordValidator()
        {
            // Hitta projektets root från där exe:n körs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Gå upp 3 nivåer: bin/Debug/net8.0 -> projekt root
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
                    _dictionary.Add(word.Trim().ToLower());
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
    }
}