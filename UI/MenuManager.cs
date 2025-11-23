using Spectre.Console;

namespace WordTrainer.UI
{
    public class MenuManager
    {
        public int ShowMenu(string title, string[] options)
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold yellow]{title}[/]")
                    .PageSize(10)
                    .AddChoices(options)
            );

            return Array.IndexOf(options, selection);
        }

        public string GetPlayerName(int playerNumber)
        {
            return AnsiConsole.Ask<string>($"[green]Spelare {playerNumber}, ange ditt namn:[/]");
        }

        public int GetPlayerCount()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[yellow]Hur många spelare?[/]")
                    .AddChoices(new[] { 2, 3, 4, 5, 6 })
            );
        }

        public void ShowMessage(string message, string color = "white")
        {
            AnsiConsole.MarkupLine($"[{color}]{message}[/]");
        }

        public void Pause()
        {
            AnsiConsole.MarkupLine("[dim]Tryck på valfri tangent för att fortsätta...[/]");
            Console.ReadKey(true);
        }
    }
}