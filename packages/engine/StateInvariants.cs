using System;
using System.Linq;

namespace Villainous.Engine;

public static class StateInvariants
{
    public static void Validate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        foreach (var player in state.Players)
        {
            if (player.Power < 0)
            {
                throw new InvalidOperationException("Power cannot be negative");
            }
            if (player.VillainDeckCount < 0)
            {
                throw new InvalidOperationException("Villain deck count cannot be negative");
            }
            if (player.FateDeckCount < 0)
            {
                throw new InvalidOperationException("Fate deck count cannot be negative");
            }
            if (player.Locations is null)
            {
                throw new InvalidOperationException("Locations cannot be null");
            }
            foreach (var location in player.Locations)
            {
                if (location is null)
                {
                    throw new InvalidOperationException("Location cannot be null");
                }
                if (location.Heroes is null || location.Heroes.Any(h => h is null))
                {
                    throw new InvalidOperationException("Heroes cannot contain null");
                }
                if (location.Allies is null || location.Allies.Any(a => a is null))
                {
                    throw new InvalidOperationException("Allies cannot contain null");
                }
            }
        }
    }
}
