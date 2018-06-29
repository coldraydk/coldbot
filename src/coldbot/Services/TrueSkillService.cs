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

        public static Rating GetDefaultRating()
        {
            return GameInfo.DefaultGameInfo.DefaultRating;
        }

        public void PlayFFA(ColdBot.Models.Magic.Command command, String channel)
        {
            var teams = new List<Team>();
            var decks = command.Decks;
            var gameMode = command.GameMode;

            var ratings = getRatings(decks, gameMode, channel);

            if (ratings == null)
                return;

            foreach (var rating in ratings)
            {
                var tsPlayer = new Player(rating.Player.Name);
                var tsRating = new Rating(rating.Mean, rating.StandardDeviation, rating.ConservativeRating);
                var tsTeam = new Team(tsPlayer, tsRating);

                teams.Add(tsTeam);
            }

            var ranking = generateRanking(teams.Count);
            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(teams.ToArray()), ranking.ToArray());

            foreach (var playerWithRating in newRatings)
            {
                var rating = ratings
                    .Where(x => x.Player.Name.ToLower().Equals(playerWithRating.Key.Id.ToString().ToLower()))
                    .Where(x => x.GameMode.Equals(gameMode))
                    .FirstOrDefault();

                // var reference = context.Ratings.Where(x => x.Equals(rating)).First();

                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }

        public void Play2V2(ColdBot.Models.Magic.Command command, String channel)
        {
            var teams = new List<Team>();
            var decks = command.Decks;
            var gameMode = command.GameMode;

            var ratings = getRatings(decks, gameMode, channel);

            if (ratings == null)
                return;

            var tsWinner1 = new Player(command.Decks[0].Player.Name);
            var tsWinner2 = new Player(command.Decks[1].Player.Name);
            var tsLoser1 = new Player(command.Decks[2].Player.Name);
            var tsLoser2 = new Player(command.Decks[3].Player.Name);

            var tsRatingWinner1 = new Rating(ratings[0].Mean, ratings[0].StandardDeviation, ratings[0].ConservativeRating);
            var tsRatingWinner2 = new Rating(ratings[1].Mean, ratings[1].StandardDeviation, ratings[1].ConservativeRating);
            var tsRatingLoser1 = new Rating(ratings[2].Mean, ratings[2].StandardDeviation, ratings[2].ConservativeRating);
            var tsRatingLoser2 = new Rating(ratings[3].Mean, ratings[3].StandardDeviation, ratings[3].ConservativeRating);

            var tsWinningTeam = new Team().AddPlayer(tsWinner1, tsRatingWinner1).AddPlayer(tsWinner2, tsRatingWinner2);
            var tsLosingTeam = new Team().AddPlayer(tsLoser1, tsRatingLoser1).AddPlayer(tsLoser2, tsRatingLoser2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(tsWinningTeam, tsLosingTeam), 1, 2);

            foreach (var playerWithRating in newRatings)
            {
                var rating = ratings
                    .Where(x => x.Player.Name.ToLower().Equals(playerWithRating.Key.Id.ToString().ToLower()))
                    .Where(x => x.GameMode.Equals(gameMode))
                    .FirstOrDefault();

                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }

        public void Play2HG(ColdBot.Models.Magic.Command command, String channel)
        {
            var teams = new List<Team>();
            var decks = command.Decks;
            var gameMode = command.GameMode;

            var ratings = getRatings(decks, gameMode, channel);

            if (ratings == null)
                return;

            var tsWinner1 = new Player(command.Decks[0].Player.Name);
            var tsWinner2 = new Player(command.Decks[1].Player.Name);
            var tsLoser1 = new Player(command.Decks[2].Player.Name);
            var tsLoser2 = new Player(command.Decks[3].Player.Name);

            var tsRatingWinner1 = new Rating(ratings[0].Mean, ratings[0].StandardDeviation, ratings[0].ConservativeRating);
            var tsRatingWinner2 = new Rating(ratings[1].Mean, ratings[1].StandardDeviation, ratings[1].ConservativeRating);
            var tsRatingLoser1 = new Rating(ratings[2].Mean, ratings[2].StandardDeviation, ratings[2].ConservativeRating);
            var tsRatingLoser2 = new Rating(ratings[3].Mean, ratings[3].StandardDeviation, ratings[3].ConservativeRating);

            var tsWinningTeam = new Team().AddPlayer(tsWinner1, tsRatingWinner1).AddPlayer(tsWinner2, tsRatingWinner2);
            var tsLosingTeam = new Team().AddPlayer(tsLoser1, tsRatingLoser1).AddPlayer(tsLoser2, tsRatingLoser2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(tsWinningTeam, tsLosingTeam), 1, 2);

            foreach (var playerWithRating in newRatings)
            {
                var rating = ratings
                    .Where(x => x.Player.Name.ToLower().Equals(playerWithRating.Key.Id.ToString().ToLower()))
                    .Where(x => x.GameMode.Equals(gameMode))
                    .FirstOrDefault();

                rating.Mean = playerWithRating.Value.Mean;
                rating.StandardDeviation = playerWithRating.Value.StandardDeviation;
                rating.ConservativeRating = playerWithRating.Value.ConservativeRating;
            }

            context.SaveChanges();
        }

        private List<ColdBot.Models.Magic.Rating> getRatings(List<ColdBot.Models.Magic.Deck> decks, ColdBot.Models.Magic.GameMode gameMode, String channel)
        {
            List<ColdBot.Models.Magic.Rating> ratings = new List<ColdBot.Models.Magic.Rating>();
            foreach (var deck in decks)
            {
                var rating = context.Ratings
                    .Include(x => x.Player)
                    .Include(x => x.Deck)
                    .Include(x => x.GameMode)
                    .Where(x => x.Player == deck.Player)
                    .Where(x => x.Deck == deck)
                    .Where(x => x.GameMode == gameMode)
                    .FirstOrDefault();

                if (rating == null)
                {
                    slackService.SendMessage($"Could not find FFA rating for {deck.Player.Name}'s {deck.DeckName} deck. This isn't good at all, since I tried to help you fix it already.", channel);
                    return null;
                }

                ratings.Add(rating);
            }

            return ratings;
        }

        private List<int> generateRanking(int count)
        {
            List<int> ranking = new List<int>();
            ranking.Add(1);

            for (int i = 2; i <= count; i++)
                ranking.Add(2);

            return ranking;
        }
    }
}