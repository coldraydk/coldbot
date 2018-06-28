using System;
using System.Collections.Generic;
using System.Linq;
using ColdBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moserware.Skills;

namespace ColdBot.Services
{
    public class TrueSkillService
    {
        private readonly SlackService slackService;
        private readonly MagicContext context;
        private readonly ILogger<TrueSkillService> logger;

        public TrueSkillService(SlackService slackService, MagicContext context, ILogger<TrueSkillService> logger)
        {
            this.slackService = slackService;
            this.context = context;
            this.logger = logger;
        }

        public Rating GetDefaultRating()
        {
            return GameInfo.DefaultGameInfo.DefaultRating;
        }

        public void PlayFFA(List<ColdBot.Models.Magic.Player> players, String channel)
        {
            List<Team> teams = new List<Team>();

            foreach (var player in players)
            {
                var tsPlayer = new Player(player.Name);
                var rating = context.Ratings
                    .Include(x => x.Player)
                    .Include(x => x.GameMode)
                    .Where(x => x.Player == player)
                    .Where(x => x.GameMode.ShortName.ToLower().Equals("ffa"))
                    .FirstOrDefault();

                if (rating == null)
                {
                    slackService.SendMessage($"Could not find FFA rating for {player.Name}. This really isn't good, since I tried to help you fix it already.", channel);
                    return;
                }

                var tsRating = new Rating(rating.Mean, rating.StandardDeviation, rating.ConservativeRating);
                var tsTeam = new Team(tsPlayer, tsRating);

                teams.Add(tsTeam);
            }

            List<int> ranking = new List<int>();
            ranking.Add(1);

            for (int i = 2; i <= teams.Count; i++)
                ranking.Add(2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(teams.ToArray()), ranking.ToArray());

            foreach (var playerWithRating in newRatings)
            {
                var rating = context.Ratings.Where(x => x.Player.Name.ToLower().Equals(playerWithRating.Key.Id.ToString().ToLower())).Where(x => x.GameMode.ShortName.ToLower().Equals("ffa")).FirstOrDefault();
                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }

        public void Play2V2(List<ColdBot.Models.Magic.Player> players, String channel)
        {
            if (players.Count != 4)
            {
                slackService.SendMessage($"Two versus two requires exactly four players. For some unknown reason, I have {players.Count} player(s).", channel);
                return;
            }

            var tsWinner1 = new Player(players[0]);
            var tsWinner2 = new Player(players[1]);
            var tsLoser1 = new Player(players[2]);
            var tsLoser2 = new Player(players[3]);

            var ratingWinner1 = context.Ratings.Where(x => x.Player == players[0]).Where(x => x.GameMode.ShortName.ToLower().Equals("2v2")).FirstOrDefault();
            var ratingWinner2 = context.Ratings.Where(x => x.Player == players[1]).Where(x => x.GameMode.ShortName.ToLower().Equals("2v2")).FirstOrDefault();
            var ratingLoser1 = context.Ratings.Where(x => x.Player == players[2]).Where(x => x.GameMode.ShortName.ToLower().Equals("2v2")).FirstOrDefault();
            var ratingLoser2 = context.Ratings.Where(x => x.Player == players[3]).Where(x => x.GameMode.ShortName.ToLower().Equals("2v2")).FirstOrDefault();

            var tsRatingWinner1 = new Rating(ratingWinner1.Mean, ratingWinner1.StandardDeviation, ratingWinner1.ConservativeRating);
            var tsRatingWinner2 = new Rating(ratingWinner2.Mean, ratingWinner2.StandardDeviation, ratingWinner2.ConservativeRating);
            var tsRatingLoser1 = new Rating(ratingLoser1.Mean, ratingLoser1.StandardDeviation, ratingLoser1.ConservativeRating);
            var tsRatingLoser2 = new Rating(ratingLoser2.Mean, ratingLoser2.StandardDeviation, ratingLoser2.ConservativeRating);

            var tsWinningTeam = new Team().AddPlayer(tsWinner1, tsRatingWinner1).AddPlayer(tsWinner2, tsRatingWinner2);
            var tsLosingTeam = new Team().AddPlayer(tsLoser1, tsRatingLoser1).AddPlayer(tsLoser2, tsRatingLoser2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(tsWinningTeam, tsLosingTeam), 1, 2);

            foreach (var playerWithRating in newRatings)
            {
                var player = (ColdBot.Models.Magic.Player)playerWithRating.Key.Id;
                var rating = context.Ratings
                    .Where(x => x.Player.Name.ToLower().Equals(player.Name.ToLower()))
                    .Where(x => x.GameMode.ShortName.ToLower().Equals("2v2"))
                    .FirstOrDefault();

                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }

        public void Play2HG(List<ColdBot.Models.Magic.Player> players, String channel)
        {
            if (players.Count != 4 && players.Count != 6)
            {
                slackService.SendMessage($"Two-Headed Giant requires exactly four or six players. For some unknown reason, I have {players.Count} player(s).", channel);
                return;
            }

            var tsWinner1 = new Player(players[0]);
            var tsWinner2 = new Player(players[1]);
            var tsLoser1 = new Player(players[2]);
            var tsLoser2 = new Player(players[3]);

            var ratingWinner1 = context.Ratings.Where(x => x.Player == players[0]).Where(x => x.GameMode.ShortName.ToLower().Equals("2hg")).FirstOrDefault();
            var ratingWinner2 = context.Ratings.Where(x => x.Player == players[1]).Where(x => x.GameMode.ShortName.ToLower().Equals("2hg")).FirstOrDefault();
            var ratingLoser1 = context.Ratings.Where(x => x.Player == players[2]).Where(x => x.GameMode.ShortName.ToLower().Equals("2hg")).FirstOrDefault();
            var ratingLoser2 = context.Ratings.Where(x => x.Player == players[3]).Where(x => x.GameMode.ShortName.ToLower().Equals("2hg")).FirstOrDefault();

            var tsRatingWinner1 = new Rating(ratingWinner1.Mean, ratingWinner1.StandardDeviation, ratingWinner1.ConservativeRating);
            var tsRatingWinner2 = new Rating(ratingWinner2.Mean, ratingWinner2.StandardDeviation, ratingWinner2.ConservativeRating);
            var tsRatingLoser1 = new Rating(ratingLoser1.Mean, ratingLoser1.StandardDeviation, ratingLoser1.ConservativeRating);
            var tsRatingLoser2 = new Rating(ratingLoser2.Mean, ratingLoser2.StandardDeviation, ratingLoser2.ConservativeRating);

            var tsWinningTeam = new Team().AddPlayer(tsWinner1, tsRatingWinner1).AddPlayer(tsWinner2, tsRatingWinner2);
            var tsLosingTeam = new Team().AddPlayer(tsLoser1, tsRatingLoser1).AddPlayer(tsLoser2, tsRatingLoser2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(tsWinningTeam, tsLosingTeam), 1, 2);

            foreach (var playerWithRating in newRatings)
            {
                var rating = context.Ratings.Include(x => x.Player).Where(x => x.Player.Name.ToLower().Equals(playerWithRating.Key.Id.ToString().ToLower())).Where(x => x.GameMode.ShortName.ToLower().Equals("2hg")).FirstOrDefault();
                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }
    }
}