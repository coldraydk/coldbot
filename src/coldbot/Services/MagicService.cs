using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ColdBot.Data;
using ColdBot.Models;
using ColdBot.Models.Magic;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ColdBot.Services
{
    public class MagicService
    {
        private readonly SlackService slackService;
        private readonly TrueSkillService trueSkillService;
        private readonly MagicContext context;
        private readonly ILogger<MagicService> logger;

        public MagicService(SlackService slackService, TrueSkillService trueSkillService, MagicContext context, ILogger<MagicService> logger)
        {
            this.slackService = slackService;
            this.trueSkillService = trueSkillService;
            this.context = context;
            this.logger = logger;
        }

        public void ProcessEvent(Event slackEvent)
        {
            var channel = slackEvent?.Channel;
            var command = Command.Parse(slackEvent?.Text, context, slackService, channel);

            if (command == null)
                return;

            switch (command.Name)
            {
                case "help":
                    handleHelp(command, channel);
                    break;
                case "player":
                case "players":
                    handlePlayers(command, channel);
                    break;
                case "game":
                    handleGame(command, channel);
                    break;
                case "random":
                    handleRandom(command, channel);
                    break;
                case "setup":
                    handleSetup(command, channel);
                    break;
            }
        }

        private void handleHelp(Command command, string channel)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("*Commands:*\n");
            sb.Append("> magic help\n");
            sb.Append("> magic players\n");
            sb.Append("> magic players add <name> [<name2>..<nameX>]\n");
            sb.Append("> magic players stats <gamemode> (ffa, 1v1, 2v2, 2hg)\n");
            sb.Append("> magic game ffa <winner:deck> <loser1:deck> <loser2:deck> [<loser3:deck>..<loserX:deck>]\n");
            sb.Append("> magic game 1v1 <winner:deck> <loser:deck>\n");
            sb.Append("> magic game 2v2 <winner1:deck> <winner2:deck> <loser1:deck> <loser2:deck>\n");
            sb.Append("> magic game 2hg <winner1:deck> <winner2:deck> <loser1:deck> <loser2:deck>\n");
            sb.Append("> magic random <player1> <player2> <player3> <player4>\n");

            slackService.SendMessage(sb.ToString(), channel);
        }

        private void handlePlayers(Command command, String channel)
        {
            StringBuilder sb = new StringBuilder();

            // List players if nothing else is supplied with the players command
            if (command.Parameters.Count == 0)
            {
                var players = context.Players.ToList();

                sb.Append("*List of players:*\n");

                foreach (var player in players)
                    sb.Append($"> {player.Name}\n");

                slackService.SendMessage(sb.ToString(), channel);

                return;
            }
            // Add player(s)
            else if (command.Parameters[0].ToLower().Equals("add"))
            {
                command.Parameters.Remove(command.Parameters.FirstOrDefault(x => x.ToLower().Equals("add")));

                if (command.Parameters.Count == 0)
                    slackService.SendMessage("I need one or more people to add.", channel);
                else
                {
                    var defaultRating = TrueSkillService.GetDefaultRating();

                    sb.Append($"*Adding players:*\n");

                    foreach (var playerName in command.Parameters)
                    {
                        if (context.Players.Where(x => x.Name.ToLower().Equals(playerName.ToLower())).FirstOrDefault() == null)
                        {
                            Player player = new Player() { Name = playerName };
                            context.Players.Add(player);

                            context.SaveChanges();

                            sb.Append($"> ..adding {playerName}.\n");
                        }
                        else
                            sb.Append($"> ..skipping {playerName} since I already know him.\n");
                    }

                    slackService.SendMessage(sb.ToString(), channel);
                }
            }
            else if (command.Parameters[0].ToLower().Equals("stats"))
            {
                GameMode gameMode = command.GameMode;

                if (gameMode == null)
                    gameMode = context.GameModes.Where(x => x.ShortName.Equals("ffa")).First();

                var rankings = context.Ratings.Where(x => x.GameMode.Equals(gameMode)).Include(x => x.Player).Include(x => x.Deck).OrderByDescending(x => x.ConservativeRating)?.ToList();

                if (rankings == null || rankings.Count == 0)
                {
                    slackService.SendMessage($"No ratings found for {gameMode.Name}.", channel);
                    return;
                }

                sb.Append($"*{gameMode.Name} rankings:*\n");

                int i = 1;
                foreach (var ranking in rankings)
                    sb.Append($"> {i++}: {ranking.Player.Name}'s {ranking.Deck.DeckName} (R={ranking.ConservativeRating.ToString("#.#")} | μ={ranking.Mean.ToString("#.#")} | σ={ranking.StandardDeviation.ToString("#.#")})\n");

                slackService.SendMessage(sb.ToString(), channel);
            }
            else
                slackService.SendMessage("This doesn't make a lot of sense to me.", channel);
        }

        private void handleGame(Command command, String channel)
        {
            // Verify that a game mode is specified
            if (command.GameMode == null)
            {
                slackService.SendMessage("Please specify a game mode.", channel);
                return;
            }

            // FFA
            if (command.GameMode.ShortName.Equals("ffa"))
            {
                if (command.Decks.Count < 3)
                {
                    slackService.SendMessage("Game mode FFA needs at least three players.", channel);
                    return;
                }
                // Prerequisites are satisfied
                else
                {
                    trueSkillService.PlayFFA(command, channel);

                    slackService.SendMessage($"..And the winner is {command.Decks[0].Player.Name}!", channel);
                }
            }
            else if (command.Parameters[0].ToLower().Equals("2v2"))
            {
                if (command.Decks.Count != 4)
                {
                    slackService.SendMessage("Usage: magic game 2v2 <winner1> <winner2> <loser1> <loser2>", channel);
                    return;
                }
                else
                {
                    trueSkillService.Play2V2(command, channel);
                    slackService.SendMessage($"Yay! Congrats {command.Decks[0].Player.Name} and {command.Decks[1].Player.Name}!", channel);
                }
            }
            else if (command.Parameters[0].ToLower().Equals("2hg"))
            {
                if (command.Decks.Count != 4)
                {
                    slackService.SendMessage("Usage: magic game 2hg <winner1> <winner2> <loser1> <loser2>", channel);
                    return;
                }
                else
                {
                    trueSkillService.Play2V2(command, channel);
                    slackService.SendMessage($"Two heads, one body! {command.Decks[0].Player.Name} and {command.Decks[1].Player.Name} are victorious!", channel);
                }
            }
        }

        private void handleRandom(Command command, String channel)
        { 
            if (command.Parameters.Count != 4)
            {
                slackService.SendMessage("This feature takes exactly four players currently.", channel);
                return;
            }

            Random rnd = new Random();

            List<GameMode> gameModes = context.GameModes.Where(x => x.ShortName.Equals("ffa") || x.ShortName.Equals("2hg") || x.ShortName.Equals("2v2")).ToList();

            var gameMode = gameModes[rnd.Next(2)];

            if (gameMode.ShortName.Equals("ffa"))
                slackService.SendMessage($"The dice has spoken: Time for a game of {gameMode.Name}. Good luck everyone!", channel);
            else
            {
                var team1 = command.Parameters.OrderBy(x => rnd.Next()).Take(2).ToList();
                var team2 = command.Parameters.Except(team1).ToList();

                slackService.SendMessage($"The dice has spoken: Time for a game of {gameMode.Name}. Teams are {team1[0]} and {team1[1]} versus {team2[0]} and {team2[1]}. May the best team win!", channel);
            }
        }


        private void handleSetup(Command command, String channel)
        {
            // Setup and seeding.
            if (command.Parameters.Count == 0)
            {
                if (context.GameModes.Count() == 0)
                {
                    context.GameModes.Add(new GameMode()
                    {
                        Name = "Free for all",
                        ShortName = "ffa",
                    });
                    context.GameModes.Add(new GameMode()
                    {
                        Name = "One versus one",
                        ShortName = "1v1",
                    });
                    context.GameModes.Add(new GameMode()
                    {
                        Name = "Two versus two",
                        ShortName = "2v2",
                    });
                    context.GameModes.Add(new GameMode()
                    {
                        Name = "Two-Headed Giant",
                        ShortName = "2hg",
                    });

                    context.SaveChanges();

                    slackService.SendMessage("Setup complete.", channel);
                }
                else
                    slackService.SendMessage("It seems that I already know several game modes. You should probably reset me first!", channel);
            }
            else if (command.Parameters.Count == 1 && command.Parameters.First().ToLower().Equals("reset"))
            {
                context.Ratings.RemoveRange(context.Ratings);
                context.MatchResults.RemoveRange(context.MatchResults);
                context.Players.RemoveRange(context.Players);
                context.GameModes.RemoveRange(context.GameModes);
                context.GameModeSnapshots.RemoveRange(context.GameModeSnapshots);
                context.Decks.RemoveRange(context.Decks);
                context.SaveChanges();
            }
        }

        // private void printScore(string channel)
        // {
        //     var sortedPlayers = context.Players.OrderByDescending(x => x.Rating).ToList();

        //     int rank = 1;

        //     StringBuilder sb = new StringBuilder();
        //     sb.Append("*Laks og Nødder's Magic League*\n");

        //     foreach (Player player in sortedPlayers)
        //     {
        //         sb.Append($"{rank}. {player.Name} (Rating: {player.Rating})\n");
        //         rank++;
        //     }

        //     slackService.SendMessage(sb.ToString(), channel);
        // }

        // private void playMatch(string player1name, string player2name, string channel)
        // {
        //     Player player1 = context.Players.Where(x => x.Name.ToLower().Equals(player1name.ToLower())).FirstOrDefault();
        //     Player player2 = context.Players.Where(x => x.Name.ToLower().Equals(player2name.ToLower())).FirstOrDefault();

        //     if (player1 == null)
        //     {
        //         slackService.SendMessage($"{player1name} is not participating in the league.", channel);
        //         return;
        //     }

        //     if (player2 == null)
        //     {
        //         slackService.SendMessage($"{player1name} is not participating in the league.", channel);
        //         return;
        //     }

        //     if (player1.Name.ToLower().Equals(player2.Name.ToLower()))
        //     {
        //         slackService.SendMessage("You need to have two unique players to actually have a match.", channel);
        //         return;
        //     }

        //     playMatch(player1, player2, channel);
        // }

        // private void printStats(string name, string channel)
        // {
        //     Player player = context.Players.Where(x => x.Name.ToLower().Equals(name.ToLower())).FirstOrDefault();

        //     if (player == null)
        //     {
        //         slackService.SendMessage("Player is not a part of the league.", channel);
        //         return;
        //     }

        //     var stats = this.getPlayerStatistics(player);

        //     StringBuilder sb = new StringBuilder();
        //     sb.Append($"*Statistics for {stats.Player.Name}:*\n");
        //     sb.Append($"Rating: {stats.Player.Rating}\n");
        //     sb.Append($"Wins: {stats.Wins} | Losses: {stats.Losses}\n");

        //     sb.Append($"{stats.Player.Name}'s last ten games:\n");

        //     if (stats.MatchHistory?.Count > 0)
        //     {
        //         foreach (MatchResult result in stats.MatchHistory)
        //         {
        //             sb.Append((result.Winner.Equals(stats.Player) ? $"> Won against {result.Loser.Name} {relativeTimeElapsed(result.Timestamp)} (+{result.RatingChange} rating)" : $"> Lost against {result.Winner.Name} {relativeTimeElapsed(result.Timestamp)} (-{result.RatingChange} rating)") + $"\n");
        //         }
        //     }
        //     else
        //         sb.Append($"> No matches found.");

        //     slackService.SendMessage(sb.ToString(), channel);
        // }

        // private void playMatch(Player winner, Player loser, string channel)
        // {
        //     Player winnerfound = context.Players.Where(x => x.Name.ToLower().Equals(winner.Name.ToLower())).FirstOrDefault();
        //     Player loserfound = context.Players.Where(x => x.Name.ToLower().Equals(loser.Name.ToLower())).FirstOrDefault();

        //     if (winnerfound == null)
        //     {
        //         slackService.SendMessage($"{winner.Name} is not participating in the league.", channel);
        //         return;
        //     }

        //     if (loserfound == null)
        //     {
        //         slackService.SendMessage($"{loser.Name} is not participating in the league.", channel);
        //         return;
        //     }

        //     int player1ELO = winnerfound.Rating;
        //     int player2ELO = loserfound.Rating;

        //     float A1 = System.Convert.ToSingle(player1ELO);
        //     float B1 = System.Convert.ToSingle(player2ELO);

        //     double addsub = 32 * (1 - (1 / (1 + System.Math.Pow(10, ((B1 - A1) / 400)))));
        //     int addsubint = System.Convert.ToInt32(addsub);

        //     MatchResult result = new MatchResult();
        //     result.Timestamp = DateTime.UtcNow;
        //     result.RatingChange = addsubint;


        //     winnerfound.Rating = player1ELO + addsubint;
        //     loserfound.Rating = player2ELO - addsubint;

        //     result.Winner = winnerfound;
        //     result.Loser = loserfound;


        //     context.MatchResults.Add(result);
        //     context.SaveChanges();

        //     slackService.SendMessage($"{winner.Name} defeated {loser.Name} (+/- {result.RatingChange} rating).", channel);
        // }

        // private void printHelp(string channel)
        // {
        //     StringBuilder sb = new StringBuilder();

        //     sb.Append("*Commands:*\n");
        //     sb.Append("league stats\n");
        //     sb.Append("league stats <player>\n");
        //     sb.Append("league <winner> vs <loser>\n");
        //     sb.Append("league addplayer <name> [<name2>..<nameX>]");

        //     slackService.SendMessage(sb.ToString(), channel);
        // }

        // private PlayerStatistics getPlayerStatistics(Player player)
        // {
        //     PlayerStatistics stats = new PlayerStatistics();

        //     var matchhistory = context.MatchResults.Where(x => x.Loser.Equals(player) || x.Winner.Equals(player)).Include(x => x.Winner).Include(x => x.Loser).OrderByDescending(x => x.Timestamp).ToList();

        //     stats.Player = player;
        //     stats.Wins = matchhistory.Where(x => x.Winner.Equals(player)).Count();
        //     stats.Losses = matchhistory.Where(x => x.Loser.Equals(player)).Count();
        //     stats.MatchHistory = matchhistory.Take(10).ToList();

        //     return stats;
        // }

        // private int getBoosterPacksForLosses(Player player, List<MatchResult> matchesPlayed)
        // {
        //     int packs = 0;
        //     int consecutiveLosses = 0;

        //     foreach (var match in matchesPlayed)
        //     {
        //         if (match.Loser.Name.ToLower().Equals(player.Name.ToLower()))
        //         {
        //             consecutiveLosses++;

        //             if (consecutiveLosses == 3)
        //             {
        //                 packs++;
        //                 consecutiveLosses = 0;
        //             }
        //         }
        //         else if (match.Winner.Name.ToLower().Equals(player.Name.ToLower()))
        //             consecutiveLosses = 0;
        //     }

        //     return packs;
        // }

        // private int getBoosterPacksForTimeElapsed()
        // {
        //     int packs = 0;

        //     foreach (var when in boosterPacksGivenWhen)
        //         if (when < DateTime.UtcNow)
        //             packs++;

        //     return packs;
        // }

        // private void reset(Event slackEvent, string channel)
        // {
        //     if (!slackEvent.User.ToLower().Equals("U20CAA72L".ToLower()))
        //     {
        //         slackService.SendMessage("You are not my owner!", channel);
        //         return;
        //     }

        //     this.league = null;
        //     boosterPacksGivenWhen = new List<DateTime>();

        //     context.MatchResults.RemoveRange(context.MatchResults);
        //     context.Players.RemoveRange(context.Players);
        //     context.League.RemoveRange(context.League);
        //     context.SaveChanges();

        //     slackService.SendMessage("*Cleaning up database:*\n> Cleared Players table.\n> Cleared Matches table.\n> Cleared League table.", channel);
        // }

        private string relativeTimeElapsed(DateTime dt)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}