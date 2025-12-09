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
        static PlayerSaveManager saveManager = new();  // ← NY!

        static void Main(string[] args)
        {
            ShowWelcome();

            while (true)
            {
                int choice = menu.ShowMenu("HUVUDMENY", new[] {
                    "🎮 Spela",
                    "📊 Visa min statistik",
                    "📈 Data (Bästa/Sämsta bokstäver)", // ← NYTT!
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
                        ShowPlayerStats();
                        break;
                    case 2:
                        ShowLetterAnalytics(); // ← NYTT!
                        break;
                    case 3:
                        ShowLeaderboard();
                        break;
                    case 4:
                        ShowSettings();
                        break;
                    case 5:
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
            // 1. Hämta/skapa spelarnamn
            string playerName = AnsiConsole.Ask<string>("[green]Vad heter du?[/]");

            // 2. Ladda eller skapa spelarprofil
            var profile = saveManager.LoadOrCreateProfile(playerName);

            // 3. Välj svårighetsgrad
            int diffChoice = menu.ShowMenu("Välj svårighetsgrad", new[] { "Easy", "Normal", "Hard" });
            string difficulty = diffChoice switch
            {
                0 => "Easy",
                1 => "Normal",
                2 => "Hard",
                _ => "Normal"
            };

            // 4. Skapa spelare med laddad profil
            Player player = new Player(playerName);
            List<Player> players = new() { player };

            // Välj spelläge
            int modeChoice = menu.ShowMenu("Välj spelläge", new[] {
                "Solo (spela tills du förlorar)",
                "Multiplayer (flera spelare)"
            });

            bool isSoloMode = (modeChoice == 0);

            if (!isSoloMode)
            {
                // Multiplayer - lägg till fler spelare
                int extraPlayers = AnsiConsole.Prompt(
                    new SelectionPrompt<int>()
                        .Title("Hur många spelare totalt?")
                        .AddChoices(new[] { 2, 3, 4, 5, 6 })
                );

                for (int i = 2; i <= extraPlayers; i++)
                {
                    string name = AnsiConsole.Ask<string>($"[green]Spelare {i}, ange ditt namn:[/]");
                    saveManager.LoadOrCreateProfile(name);
                    players.Add(new Player(name));
                }
            }

            // 5. Starta match (spara starttid)
            DateTime matchStart = DateTime.Now;
            GameSession session = new(players, difficulty);

            // Dictionary för att spåra prestanda per spelare
            var playerPerformances = new Dictionary<Player, List<RoundPerformance>>();
            foreach (var p in players)
            {
                playerPerformances[p] = new List<RoundPerformance>();
            }

            // 6. Huvudspelloop
            if (isSoloMode)
            {
                // SOLO MODE - spela tills du förlorar alla liv
                while (player.IsAlive)
                {
                    Round round = engine.StartNewRound(session);
                    display.ShowGameState(session, round);

                    // Hämta input från spelare
                    AnsiConsole.MarkupLine($"\n[cyan]{round.CurrentPlayer.Name}s tur![/]");
                    string input = AnsiConsole.Ask<string>("[green]Skriv ett ord:[/]").Trim();

                    // Validera svar
                    var (success, message) = engine.ValidateAnswer(input, round);

                    // Spara rundprestanda
                    var performance = new RoundPerformance
                    {
                        Letters = round.LetterCombination,
                        Success = success,
                        WordUsed = success ? input : null,
                        TimeRemaining = round.RemainingTime()
                    };
                    playerPerformances[round.CurrentPlayer].Add(performance);

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

                        if (!player.IsAlive)
                        {
                            AnsiConsole.MarkupLine("\n[red bold]💀 GAME OVER! 💀[/]");
                            AnsiConsole.MarkupLine($"[yellow]Du överlevde {session.RoundNumber - 1} rundor med {player.Score} poäng![/]");
                        }
                    }

                    System.Threading.Thread.Sleep(1500);
                    session.RoundNumber++;
                }
            }
            else
            {
                // MULTIPLAYER MODE - spela tills bara en kvar
                while (session.GetAlivePlayers().Count > 1)
                {
                    Round round = engine.StartNewRound(session);
                    display.ShowGameState(session, round);

                    // Hämta input från spelare
                    AnsiConsole.MarkupLine($"\n[cyan]{round.CurrentPlayer.Name}s tur![/]");
                    string input = AnsiConsole.Ask<string>("[green]Skriv ett ord:[/]").Trim();

                    // Validera svar
                    var (success, message) = engine.ValidateAnswer(input, round);

                    // Spara rundprestanda
                    var performance = new RoundPerformance
                    {
                        Letters = round.LetterCombination,
                        Success = success,
                        WordUsed = success ? input : null,
                        TimeRemaining = round.RemainingTime()
                    };
                    playerPerformances[round.CurrentPlayer].Add(performance);

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

                // 7. Visa vinnare (bara i multiplayer)
                var winner = session.GetWinner();
                if (winner != null)
                {
                    display.ShowWinner(winner);
                    leaderboard.AddEntry(winner, difficulty);
                }
            }

            // 8. SPARA MATCHDATA FÖR ALLA SPELARE! ← NYTT!
            AnsiConsole.MarkupLine("\n[yellow]💾 Sparar matchdata...[/]");
            foreach (var p in players)
            {
                saveManager.SaveMatch(p, session, matchStart, playerPerformances[p]);
            }

            menu.Pause();
        }

        static void ShowPlayerStats()
        {
            // Lista alla spelare eller fråga om namn
            var allPlayers = saveManager.GetAllPlayers();

            if (allPlayers.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Inga sparade spelare ännu. Spela först![/]");
                menu.Pause();
                return;
            }

            string selectedPlayer;

            if (allPlayers.Count == 1)
            {
                selectedPlayer = allPlayers[0];
            }
            else
            {
                selectedPlayer = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Välj spelare:[/]")
                        .AddChoices(allPlayers)
                );
            }

            AnsiConsole.Clear();
            saveManager.ShowPlayerStats(selectedPlayer);
            menu.Pause();
        }

        static void ShowLetterAnalytics()
        {
            var allPlayers = saveManager.GetAllPlayers();

            if (allPlayers.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Inga sparade spelare ännu. Spela först![/]");
                menu.Pause();
                return;
            }

            string selectedPlayer;

            if (allPlayers.Count == 1)
            {
                selectedPlayer = allPlayers[0];
            }
            else
            {
                selectedPlayer = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Välj spelare:[/]")
                        .AddChoices(allPlayers)
                );
            }

            AnsiConsole.Clear();
            saveManager.ShowLetterAnalytics(selectedPlayer);
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
            AnsiConsole.Clear();

            int choice = menu.ShowMenu("INSTÄLLNINGAR", new[] {
                "Visa alla spelare",
                "Radera spelarprofil",
                "Tillbaka"
            });

            switch (choice)
            {
                case 0:
                    ShowAllPlayers();
                    break;
                case 1:
                    DeletePlayerProfile();
                    break;
            }
        }

        static void ShowAllPlayers()
        {
            var players = saveManager.GetAllPlayers();

            if (players.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Inga sparade spelare.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]📋 ALLA SPELARE:[/]\n");
                foreach (var player in players)
                {
                    AnsiConsole.MarkupLine($"  • {player}");
                }
            }

            menu.Pause();
        }

        static void DeletePlayerProfile()
        {
            var players = saveManager.GetAllPlayers();

            if (players.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Inga spelare att radera.[/]");
                menu.Pause();
                return;
            }

            string toDelete = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Vilken spelare vill du radera?[/]")
                    .AddChoices(players.Concat(new[] { "Avbryt" }))
            );

            if (toDelete != "Avbryt")
            {
                bool confirm = AnsiConsole.Confirm($"Är du säker på att radera {toDelete}?");
                if (confirm)
                {
                    saveManager.DeletePlayer(toDelete);
                }
            }

            menu.Pause();
        }
    }
}