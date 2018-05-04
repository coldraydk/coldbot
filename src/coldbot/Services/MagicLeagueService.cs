using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LakseBot.Data;
using LakseBot.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LakseBot.Services
{
    public class MagicLeagueService
    {
        private readonly SlackService slackService;
        private readonly MagicLeagueContext context;
        private readonly ILogger<MagicLeagueService> logger;
        private League league;
        private List<DateTime> boosterPacksGivenWhen = new List<DateTime>();

        public MagicLeagueService(SlackService slackService, MagicLeagueContext context, ILogger<MagicLeagueService> logger)
        {
            this.slackService = slackService;
            this.context = context;
            this.logger = logger;

            this.league = context.League.FirstOrDefault();

            if (this.league != null)
                setPackIntervalBasedOnGameMode(league.GameMode);
        }

        public void ProcessEvent(Event slackEvent)
        {
            var commands = slackEvent?.Text?.Split(" ");
            var channel = slackEvent?.Channel;

            if (commands == null || commands.Length == 0 || !commands[0].ToLower().Equals("league"))
                return;

            // Is the league running? If not, we should definitely try to start one.
            if (league == null && commands.Length >= 2 && commands[1].ToLower().Contains("start"))
            {
                var league = new League();
                league.Name = $"Laks og Nødder's Magic League";
                league.StartTime = DateTime.UtcNow;

                if (commands.Length == 3 && commands[2].Equals("dev"))
                    league.GameMode = 1;
                else
                    league.GameMode = 0;

                logger.LogInformation("Adding league to DB.");

                context.League.Add(league);
                context.SaveChanges();

                logger.LogInformation("Changes saved.");

                StringBuilder leagueAnnouncement = new StringBuilder();

                leagueAnnouncement.Append($"*Started new league: {league.Name}*\n\n");

                setPackIntervalBasedOnGameMode(league.GameMode);

                leagueAnnouncement.Append($"Extra packs will be purchasable at:\n");

                foreach (var when in boosterPacksGivenWhen)
                    leagueAnnouncement.Append($"> {when.ToString()}\n");

                slackService.SendMessage(leagueAnnouncement.ToString(), channel);

                this.league = league;
                logger.LogInformation("League assigned to local variable this.league");

                return;
            }
            else if (league != null && commands[1].ToLower().Contains("start"))
            {
                slackService.SendMessage($"{league.Name} is already running. It started {relativeTimeElapsed(league.StartTime)}.", channel);
                return;
            }

            if (league == null)
            {
                slackService.SendMessage("No league is currently running.", channel);
                return;
            }

            // League is running. Let's do something with the commands that are given.
            if (commands[1].ToLower().Equals("addplayer") && commands.Length >= 3)
                addPlayer(commands, channel);
            else if (commands.Length == 4 && commands[2].ToLower().Equals("vs"))
                playMatch(commands[1], commands[3], channel);
            else if (commands.Length == 2 && (commands[1].ToLower().Equals("score") || commands[1].ToLower().Equals("scoreboard") || commands[1].ToLower().Equals("scores") || commands[1].ToLower().Equals("history") || commands[1].ToLower().Equals("stats")))
                printScore(channel);
            else if (commands.Length == 3 && (commands[1].ToLower().Equals("stats") || commands[1].ToLower().Equals("history")))
                printStats(commands[2], channel);
            else if (commands.Length == 2 && commands[1].ToLower().Contains("help"))
                printHelp(channel);
            else if (commands.Length == 2 && commands[1].ToLower().Contains("reset"))
                reset(slackEvent, channel);
            else
                slackService.SendMessage("That command I do not know.", channel);
        }

        private void addPlayer(string[] commands, string channel)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 2; i < commands.Length; i++)
            {
                Player player = context.Players.Where(x => x.Name.ToLower().Equals(commands[i].ToLower())).FirstOrDefault();

                if (player != null)
                {
                    sb.Append($"{commands[i]} already exists in the league.\n");
                }
                else
                {
                    context.Players.Add(new Player { Name = commands[i], Rating = 1500 });
                    sb.Append($"Added player {commands[i]} to the league.\n");
                }
            }

            context.SaveChanges();

            slackService.SendMessage(sb.ToString(), channel);
        }

        private void printScore(string channel)
        {
            var sortedPlayers = context.Players.OrderByDescending(x => x.Rating).ToList();

            int rank = 1;

            StringBuilder sb = new StringBuilder();
            sb.Append("*Laks og Nødder's Magic League*\n");

            foreach (Player player in sortedPlayers)
            {
                sb.Append($"{rank}. {player.Name} (Rating: {player.Rating})\n");
                rank++;
            }

            slackService.SendMessage(sb.ToString(), channel);
        }

        private void playMatch(string player1name, string player2name, string channel)
        {
            Player player1 = context.Players.Where(x => x.Name.ToLower().Equals(player1name.ToLower())).FirstOrDefault();
            Player player2 = context.Players.Where(x => x.Name.ToLower().Equals(player2name.ToLower())).FirstOrDefault();

            if (player1 == null)
            {
                slackService.SendMessage($"{player1name} is not participating in the league.", channel);
                return;
            }

            if (player2 == null)
            {
                slackService.SendMessage($"{player1name} is not participating in the league.", channel);
                return;
            }

            if (player1.Name.ToLower().Equals(player2.Name.ToLower()))
            {
                slackService.SendMessage("You need to have two unique players to actually have a match.", channel);
                return;
            }

            playMatch(player1, player2, channel);
        }

        private void printStats(string name, string channel)
        {
            Player player = context.Players.Where(x => x.Name.ToLower().Equals(name.ToLower())).FirstOrDefault();

            if (player == null)
            {
                slackService.SendMessage("Player is not a part of the league.", channel);
                return;
            }

            var stats = this.getPlayerStatistics(player);

            StringBuilder sb = new StringBuilder();
            sb.Append($"*Statistics for {stats.Player.Name}:*\n");
            sb.Append($"Rating: {stats.Player.Rating}\n");
            sb.Append($"Wins: {stats.Wins} | Losses: {stats.Losses}\n");
            sb.Append($"Booster packs: {stats.BoosterPacksDefault + stats.BoosterPacksLosses + stats.BoosterPacksTimeElapsed}\n");
            sb.Append($"> From the start: {stats.BoosterPacksDefault}\n");
            sb.Append($"> From league duration: {stats.BoosterPacksTimeElapsed}\n");
            sb.Append($"> From consecutive losses: {stats.BoosterPacksLosses}\n\n");

            sb.Append($"{stats.Player.Name}'s last ten games:\n");

            if (stats.MatchHistory?.Count > 0)
            {
                foreach (MatchResult result in stats.MatchHistory)
                {
                    sb.Append((result.Winner.Equals(stats.Player) ? $"> Won against {result.Loser.Name} {relativeTimeElapsed(result.Timestamp)} (+{result.RatingChange} rating)" : $"> Lost against {result.Winner.Name} {relativeTimeElapsed(result.Timestamp)} (-{result.RatingChange} rating)") + $"\n");
                }
            }
            else
                sb.Append($"> No matches found.");

            slackService.SendMessage(sb.ToString(), channel);
        }

        private void playMatch(Player winner, Player loser, string channel)
        {
            Player winnerfound = context.Players.Where(x => x.Name.ToLower().Equals(winner.Name.ToLower())).FirstOrDefault();
            Player loserfound = context.Players.Where(x => x.Name.ToLower().Equals(loser.Name.ToLower())).FirstOrDefault();

            if (winnerfound == null)
            {
                slackService.SendMessage($"{winner.Name} is not participating in the league.", channel);
                return;
            }

            if (loserfound == null)
            {
                slackService.SendMessage($"{loser.Name} is not participating in the league.", channel);
                return;
            }

            int player1ELO = winnerfound.Rating;
            int player2ELO = loserfound.Rating;

            float A1 = System.Convert.ToSingle(player1ELO);
            float B1 = System.Convert.ToSingle(player2ELO);

            double addsub = 32 * (1 - (1 / (1 + System.Math.Pow(10, ((B1 - A1) / 400)))));
            int addsubint = System.Convert.ToInt32(addsub);

            MatchResult result = new MatchResult();
            result.Timestamp = DateTime.UtcNow;
            result.RatingChange = addsubint;


            winnerfound.Rating = player1ELO + addsubint;
            loserfound.Rating = player2ELO - addsubint;

            result.Winner = winnerfound;
            result.Loser = loserfound;


            context.MatchResults.Add(result);
            context.SaveChanges();

            slackService.SendMessage($"{winner.Name} defeated {loser.Name} (+/- {result.RatingChange} rating).", channel);
        }

        private void printHelp(string channel)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("*Commands:*\n");
            sb.Append("league stats\n");
            sb.Append("league stats <player>\n");
            sb.Append("league <winner> vs <loser>\n");
            sb.Append("league addplayer <name> [<name2>..<nameX>]");

            slackService.SendMessage(sb.ToString(), channel);
        }

        private PlayerStatistics getPlayerStatistics(Player player)
        {
            PlayerStatistics stats = new PlayerStatistics();

            var matchhistory = context.MatchResults.Where(x => x.Loser.Equals(player) || x.Winner.Equals(player)).Include(x => x.Winner).Include(x => x.Loser).OrderByDescending(x => x.Timestamp).ToList();

            stats.Player = player;
            stats.Wins = matchhistory.Where(x => x.Winner.Equals(player)).Count();
            stats.Losses = matchhistory.Where(x => x.Loser.Equals(player)).Count();
            stats.MatchHistory = matchhistory.Take(10).ToList();
            stats.BoosterPacksDefault = 3;
            stats.BoosterPacksTimeElapsed = getBoosterPacksForTimeElapsed();
            stats.BoosterPacksLosses = getBoosterPacksForLosses(player, matchhistory);

            return stats;
        }

        private int getBoosterPacksForLosses(Player player, List<MatchResult> matchesPlayed)
        {
            int packs = 0;
            int consecutiveLosses = 0;

            foreach (var match in matchesPlayed)
            {
                if (match.Loser.Name.ToLower().Equals(player.Name.ToLower()))
                {
                    consecutiveLosses++;

                    if (consecutiveLosses == 3)
                    {
                        packs++;
                        consecutiveLosses = 0;
                    }
                }
                else if (match.Winner.Name.ToLower().Equals(player.Name.ToLower()))
                    consecutiveLosses = 0;
            }

            return packs;
        }

        private int getBoosterPacksForTimeElapsed()
        {
            int packs = 0;

            foreach (var when in boosterPacksGivenWhen)
                if (when < DateTime.UtcNow)
                    packs++;

            return packs;
        }

        private void reset(Event slackEvent, string channel)
        {
            if (!slackEvent.User.ToLower().Equals("U20CAA72L".ToLower()))
            {
                slackService.SendMessage("You are not my owner!", channel);
                return;
            }
            
            this.league = null;
            boosterPacksGivenWhen = new List<DateTime>();

            context.MatchResults.RemoveRange(context.MatchResults);
            context.Players.RemoveRange(context.Players);
            context.League.RemoveRange(context.League);
            context.SaveChanges();

            slackService.SendMessage("*Cleaning up database:*\n> Cleared Players table.\n> Cleared Matches table.\n> Cleared League table.", channel);
        }

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

        private void setPackIntervalBasedOnGameMode(int gamemode)
        {
            if (gamemode == 0)
            {
                boosterPacksGivenWhen.Clear();
                boosterPacksGivenWhen.Add(new DateTime(2018, 05, 11, 8, 0, 0, DateTimeKind.Utc));
                boosterPacksGivenWhen.Add(new DateTime(2018, 05, 12, 8, 0, 0, DateTimeKind.Utc));
                boosterPacksGivenWhen.Add(new DateTime(2018, 05, 13, 8, 0, 0, DateTimeKind.Utc));
            }
            else if (gamemode == 1)
            {
                boosterPacksGivenWhen.Clear();
                boosterPacksGivenWhen.Add(DateTime.UtcNow.AddHours(1));
                boosterPacksGivenWhen.Add(DateTime.UtcNow.AddHours(0));
                boosterPacksGivenWhen.Add(DateTime.UtcNow.AddHours(-1));
            }
        }
        

        private enum Winner { Player1, Player2 };
    }
}