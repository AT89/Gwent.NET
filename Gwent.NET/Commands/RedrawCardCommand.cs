﻿using System.Linq;
using Gwent.NET.Events;
using Gwent.NET.Exceptions;
using Gwent.NET.Extensions;
using Gwent.NET.Model;
using Gwent.NET.Model.States;

namespace Gwent.NET.Commands
{
    public class RedrawCardCommand : Command
    {
        public long CardId { get; set; }

        public override void Execute(Game game)
        {
            RedrawState state = game.State as RedrawState;
            if (state == null)
            {
                throw new CommandException();
            }

            Player sender = game.GetPlayerByUserId(SenderUserId);
            Player opponent = game.GetOpponentPlayerByUserId(SenderUserId);
            if (sender == null || opponent == null)
            {
                throw new CommandException();
            }

            var substate = state.Substates.FirstOrDefault(s => s.UserId == sender.User.Id);
            var opponentSubstate = state.Substates.FirstOrDefault(s => s.UserId == opponent.User.Id);
            if (substate == null || opponentSubstate == null || substate.RedrawCardCount == 0)
            {
                throw new CommandException();
            }

            if (sender.HandCards.All(c => c.Card.Id != CardId))
            {
                throw new CommandException();
            }

            RedrawCard(sender);
            substate.RedrawCardCount -= 1;
            sender.IsTurn = substate.RedrawCardCount > 0;

            Events.Add(new HandChangedEvent(sender.User.Id)
            {
                HandCards = sender.HandCards.Select(c => c.Card.Id).ToList()
            });
            
            if (substate.RedrawCardCount != 0 || opponentSubstate.RedrawCardCount != 0)
            {
                return;
            }

            NextState = new RoundState();
        }

        private void RedrawCard(Player player)
        {
            var card = player.HandCards.First(c => c.Card.Id == CardId);
            player.HandCards.Remove(card);
            
            var deckCards = player.DeckCards.ToList();
            var drawnCard = deckCards.First();
            deckCards.Remove(drawnCard);
            player.HandCards.Add(drawnCard);

            deckCards.Add(card);
            deckCards.Shuffle();
            player.DeckCards.Clear();
            player.DeckCards.AddRange(deckCards);
        }
    }
}