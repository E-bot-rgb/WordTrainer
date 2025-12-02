using Spectre.Console;
using WordTrainer.Models;
using WordTrainer.Services;
using WordTrainer.UI;

namespace WordTrainer
{
    internal class Program
    {
        static MenuManager menu = new();
        static GameDisplay display = new();
        static GameEngine engine = new();
        static LeaderboardManager leaderboard = new();

        static void Main(string[] args)
        {
            ShowWelcome();

            while (true)
            {
                int choice = menu.ShowMenu("HUVUDMENY", new[] {
                    "🎮 Spela",
                    "🏆 Topplista",
                    "⚙️ Inställningar",
                    "🚪 Avsluta"
                });

                switch (choice)
                {
                    case 0:
                        PlayGame();
                        break;
                    case 1:
                        ShowLeaderboard();
                        break;
                    case 2:
                        ShowSettings();
                        break;
                    case 3:
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("[yellow]Tack för att du spelade! 👋[/]");
                        return;
                }
            }
        }

        static void ShowWelcome()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("BOMB PARTY")
                    .Centered()
                    .Color(Color.Red));
            AnsiConsole.MarkupLine("[dim]Tryck på valfri tangent för att börja...[/]");
            Console.ReadKey(true);
        }

        static void PlayGame()
        {
            // Välj svårighetsgrad
            int diffChoice = menu.ShowMenu("Välj svårighetsgrad", new[] { "Easy", "Normal", "Hard" });
            string difficulty = diffChoice switch
            {
                0 => "Easy",
                1 => "Normal",
                2 => "Hard",
                _ => "Normal"
            };

            // Lägg till spelare
            int playerCount = menu.GetPlayerCount();
            List<Player> players = new();

            for (int i = 1; i <= playerCount; i++)
            {
                string name = menu.GetPlayerName(i);
                players.Add(new Player(name));
            }

            // Starta spelsession
            GameSession session = new(players, difficulty);

            // Huvudspelloop
            while (session.GetAlivePlayers().Count > 1)
            {
                Round round = engine.StartNewRound(session);
                display.ShowGameState(session, round);

                // Hämta input från spelare
                AnsiConsole.MarkupLine($"\n[cyan]{round.CurrentPlayer.Name}s tur![/]");
                string input = AnsiConsole.Ask<string>("[green]Skriv ett ord:[/]").Trim();

                // Validera svar
                var (success, message) = engine.ValidateAnswer(input, round);

                if (success)
                {
                    int points = engine.CalculateScore(round, input);
                    round.CurrentPlayer.AddScore(points);
                    round.CurrentPlayer.AddUsedWord(input);
                    display.ShowRoundResult(true, message, points);
                }
                else
                {
                    round.CurrentPlayer.LoseLife();
                    display.ShowRoundResult(false, message);
                }

                System.Threading.Thread.Sleep(1500);
                session.NextPlayer();
                session.RoundNumber++;
            }

            // Visa vinnare
            var winner = session.GetWinner();
            if (winner != null)
            {
                display.ShowWinner(winner);
                leaderboard.AddEntry(winner, difficulty);
            }

            menu.Pause();
        }

        static void ShowLeaderboard()
        {
            var entries = leaderboard.GetTopScores();
            display.ShowLeaderboard(entries);
            menu.Pause();
        }

        static void ShowSettings()
        {
            AnsiConsole.MarkupLine("[yellow]Inställningar kommer snart...[/]");
            menu.Pause();
        }
    }
}