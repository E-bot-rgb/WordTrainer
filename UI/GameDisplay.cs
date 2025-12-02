using Spectre.Console;
using WordTrainer.Models;

namespace WordTrainer.UI
{
    public class GameDisplay
    {
        public void ShowGameState(GameSession session, Round round)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("[yellow]Spelare[/]")
                .AddColumn("[red]Liv[/]")
                .AddColumn("[green]Poäng[/]");

            foreach (var player in session.Players)
            {
                string hearts = new string('❤', player.Lives) + new string('❤', 3 - player.Lives);
                string nameColor = player == session.CurrentPlayer ? "bold cyan" : "white";

                table.AddRow(
                    $"[{nameColor}]{player.Name}[/]",
                    hearts,
                    player.Score.ToString()
                );
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]🎮 BOMB PARTY - RUNDA {session.RoundNumber} 💣[/]"));
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            var panel = new Panel($"[bold green]Bokstäver: {round.LetterCombination}[/]\n[dim]Tid Kvar: {round.RemainingTime()}s[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Yellow);

            AnsiConsole.Write(panel);
        }

        public void ShowRoundResult(bool success, string message, int pointsEarned = 0)
        {
            if (success)
            {
                AnsiConsole.MarkupLine($"[bold green]{message} (+{pointsEarned} points)[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold red]{message}[/]");
            }
        }

        public void ShowWinner(Player winner)
        {
            var panel = new Panel(
                new FigletText("Winner!")
                    .Centered()
                    .Color(Color.Gold1))
                .Border(BoxBorder.Double)
                .BorderColor(Color.Gold1);

            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine($"\n[bold yellow]🏆 {winner.Name} won with {winner.Score} points! 🏆[/]\n");
        }

        public void ShowLeaderboard(List<Services.LeaderboardEntry> entries)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]🏆 LEADERBOARD 🏆[/]"));

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Rank")
                .AddColumn("Namn")
                .AddColumn("Poäng")
                .AddColumn("Svårighet")
                .AddColumn("Datum");

            int rank = 1;
            foreach (var entry in entries)
            {
                table.AddRow(
                    $"{rank}.",
                    entry.PlayerName,
                    entry.Score.ToString(),
                    entry.Difficulty,
                    entry.Date.ToShortDateString()
                );
                rank++;
            }

            AnsiConsole.Write(table);
        }
    }
}