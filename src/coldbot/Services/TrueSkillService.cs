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
            // TODO: Verify data.
            Play(command, channel);
        }

        public void Play2V2(ColdBot.Models.Magic.Command command, String channel)
        {
            // TODO: Verify data.
            Play(command, channel);
        }

        public void Play2HG(ColdBot.Models.Magic.Command command, String channel)
        {
            // TODO: Verify data.
            Play(command, channel);
        }

        public void Play(ColdBot.Models.Magic.Command command, String channel)
        {
            var teams = new List<Team>();
            var decks = command.Decks;
            var gameMode = command.GameMode;
            var ratings = getRatings(decks, gameMode, channel);
            int playersPerTeam = command.GameMode.ShortName.Equals("ffa") ? 1 : 2;

            if (ratings == null)
            {
                slackService.SendMessage("Rating were not properly fetched. The game was not recorded.", channel);
                return;
            }

            if (decks.Count % playersPerTeam != 0)
            {
                slackService.SendMessage("Actual numbers players are not divisible by players per team. The game was not recorded.", channel);
                return;
            }

            var playersWithRating = new List<Tuple<Player, Rating>>();

            // Player names and their rating from the database to TrueSkill objects
            for (int i = 0; i < decks.Count; i++)
            {
                var player = new Player(decks[i].Player.Name);
                var rating = new Rating(ratings[i].Mean, ratings[i].StandardDeviation);
                playersWithRating.Add((new Tuple<Player, Rating>(player, rating)));
            }

            // Divide players to teams
            while (playersWithRating.Count > 0)
            {
                var playersInTeam = new List<Tuple<Player, Rating>>();

                for (int i = 0; i < playersPerTeam; i++)
                {
                    playersInTeam.Add(new Tuple<Player, Rating>(playersWithRating[0].Item1, playersWithRating[0].Item2));
                    playersWithRating.RemoveAt(0);
                }

                var team = new Team();

                foreach (var player in playersInTeam)
                    team.AddPlayer(player.Item1, player.Item2);

                teams.Add(team);
            }

            // Calculate new rating
            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, Teams.Concat(teams.ToArray()), generateRanking(teams.Count).ToArray());

            // For each new rating match with fetched ratings and update values in DB
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

        private List<int> generateRanking(int teamCount)
        {
            List<int> ranking = new List<int>();
            ranking.Add(1);

            for (int i = 2; i <= teamCount; i++)
                ranking.Add(2);

            return ranking;
        }
    }
}