using System;
using System.Collections.Generic;
using System.IO;

namespace WordTrainer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                int mainChoice = ShowMenu("Main Menu", new string[] { "Play", "Exit" });

                if (mainChoice == 0) // Play
                {
                    int difficultyChoice = ShowMenu("Select Difficulty", new string[] { "Easy", "Normal", "Hard" });
                    string difficulty = GetDifficultyName(difficultyChoice);

                    string letters = GenerateLetters(difficulty);
                    Console.Clear();
                    Console.WriteLine($"Generated Letters ({difficulty}): {letters}");

                    Console.Write("Enter part of a word to search: ");
                    string GivenWord = Console.ReadLine();

                    var getWords = GetWords(GivenWord);
                    Console.WriteLine("\nWords containing the given string:");
                    Console.WriteLine(getWords.Count > 0 ? string.Join(", ", getWords) : "No matches found.");

                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey(true);
                }
                else if (mainChoice == 1) // Exit
                {
                    Console.Clear();
                    Console.WriteLine("Goodbye!");
                    break;
                }
            }
        }

        static int ShowMenu(string title, string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine(title + ":\n");

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        static string GetDifficultyName(int index)
        {
            return index switch
            {
                0 => "Easy",
                1 => "Normal",
                2 => "Hard",
                _ => "Unknown"
            };
        }

        static List<string> GetWords(string input)
        {
            string path = "Words.txt";
            List<string> foundWords = new();

            foreach (string word in File.ReadLines(path))
            {
                if (word.Contains(input, StringComparison.OrdinalIgnoreCase))
                {
                    foundWords.Add(word);
                }
            }

            return foundWords;
        }

        static string GenerateLetters(string difficulty)
        {
            Random rand = new();
            string path = "found.txt";
            List<string> words = new(File.ReadLines(path));

            // Difficulty can affect how letters are selected.
            // This is a placeholder; adjust logic as needed.
            string selectedWord = difficulty switch
            {
                "Easy" => words[rand.Next(words.Count)].Substring(0, Math.Min(3, words[0].Length)),
                "Normal" => words[rand.Next(words.Count)],
                "Hard" => ShuffleString(words[rand.Next(words.Count)]),
                _ => words[rand.Next(words.Count)]
            };

            return selectedWord;
        }

        static string ShuffleString(string input)
        {
            Random rand = new();
            char[] array = input.ToCharArray();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }
    }
}
