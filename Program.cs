namespace Dict
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var letters = GenerateLetters("");
            Console.WriteLine(letters);
            var GivenWord = Console.ReadLine();
            List<string> listedLetters = new List<string>([letters]);
            var getWords = GetWord(listedLetters[0]);
            Console.WriteLine("Words containing the given string:");
            Console.WriteLine(string.Join(", ", getWords));
        }
        static List <string> GetWord(string args)
        {
            string path = "Words.txt";
            List<string> words = new List<string>(File.ReadLines(path));
            List<string> foundWords = new List<string>();
            
            foreach (string word in words)
            {
                if (word.Contains(args))
                {
                    foundWords.Add(word);
                }
            }
            if (foundWords.Count == 0)
            {
                return foundWords;
            }
            return foundWords;
        }

        static string GenerateLetters(string args)
        {
            Random rand = new Random();
            string path = "found.txt";
            List<string> words = new List<string>(File.ReadLines(path));
            int randomIndex = rand.Next(words.Count);

            string madeLetters = words[randomIndex];



            return madeLetters;
        }

    }
}
