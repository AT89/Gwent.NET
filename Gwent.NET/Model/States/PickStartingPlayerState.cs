﻿using System.Collections.Generic;
using Gwent.NET.Events;
using Gwent.NET.Model.Enums;
using Gwent.NET.Model.States.Substates;

namespace Gwent.NET.Model.States
{
    public class PickStartingPlayerState : State
    {
        public virtual ICollection<PickStartingPlayerSubstate> Substates { get; set; }
        
        public override string Name
        {
            get { return "PickStartingPlayer"; }
        }

        public PickStartingPlayerState()
        {
            Substates = new HashSet<PickStartingPlayerSubstate>();
        }
        
        public override IEnumerable<Event> Initialize(Game game)
        {
            foreach (var player in game.Players)
            {
                bool canPickStartingPlayer = player.Deck.Faction == GwintFaction.Scoiatael;
                player.IsTurn = canPickStartingPlayer;
                Substates.Add(new PickStartingPlayerSubstate
                {
                    UserId = player.User.Id,
                    CanPickStartingPlayer = canPickStartingPlayer
                });
            }
            yield break;
        }
    }
}
