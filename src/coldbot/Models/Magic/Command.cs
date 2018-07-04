using System;
using System.Collections.Generic;
using System.Linq;
using ColdBot.Data;
using ColdBot.Models.Magic;
using ColdBot.Services;

namespace ColdBot.Models.Magic
{
    public class Command
    {
        public String Name { get; set; }
        public List<Deck> Decks { get; set; }
        public GameMode GameMode { get; set; }
        public List<String> Parameters { get; set; }

        public Command()
        {
            this.Parameters = new List<String>();
            this.Decks = new List<Deck>();
        }

        public static Command Parse(string input, MagicContext context, SlackService slackService, String channel)
        {
            if (input == null)
                return null;

            Command command = new Command();

            var splittedInput = input.Split(" ").ToList();

            if (!splittedInput.First().ToLower().Equals("magic") || splittedInput.Count < 2)
                return null;

            splittedInput.Remove(splittedInput.First());

            command.Name = splittedInput.First().ToLower();
            splittedInput.Remove(splittedInput.First());

            foreach (var part in splittedInput)
            {
                if (part.Contains(":"))
                {
                    var playerAndDeck = part.Split(":");

                    var playerName = playerAndDeck[0];
                    var deckName = playerAndDeck[1];

                    Player player = context.Players.Where(x => x.Name.ToLower().Equals(playerName.ToLower())).FirstOrDefault();

                    if (player == null) {
                        slackService.SendMessage($"Could not find {playerName}. You need to add him explicitly.", channel);
                        return null;
                    }

                    Deck deck = context.Decks
                        .Where(x => x.DeckName.ToLower().Equals(deckName.ToLower()))
                        .Where(x => x.Player.Equals(player))
                        .FirstOrDefault();

                    if (deck == null)
                    {
                        deck = new Deck(deckName, player);
                        context.Decks.Add(deck);
                    }

                    context.SaveChanges();
                    command.Decks.Add(deck);
                }
                else
                {
                    if (command.GameMode == null)
                        command.GameMode = context.GameModes.Where(x => x.ShortName.ToLower().Equals(part.ToLower())).FirstOrDefault();
                    
                    command.Parameters.Add(part);
                }
            }

            if (command.GameMode == null && command.Decks.Count > 0)
            {
                slackService.SendMessage("Unknown game mode!", channel);
                return null;
            }

            foreach (var deck in command.Decks)
            {
                // Verify that the player's deck has an actual rating for the game mode. Otherwise create it.
                if (context.Ratings
                    .Where(x => x.Player.Equals(deck.Player))
                    .Where(x => x.Deck.Equals(deck))
                    .Where(x => x.GameMode == command.GameMode)
                    .FirstOrDefault() == null)
                {
                    var defaultRating = TrueSkillService.GetDefaultRating();
                    context.Ratings.Add(new Rating()
                    {
                        Player = deck.Player,
                        Deck = deck,
                        GameMode = command.GameMode,
                        Mean = defaultRating.Mean,
                        StandardDeviation = defaultRating.StandardDeviation,
                        ConservativeRating = defaultRating.ConservativeRating

                    });

                    context.SaveChanges();
                }
            }

            return command;
        }
    }
}