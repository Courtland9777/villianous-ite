using System.Linq;
using Villainous.Engine;
using Villainous.Model;

namespace Villainous.Api;

public static class GameStateExtensions
{
    public static GameStateDto ToDto(this GameState state) => new(
        state.MatchId,
        state.Players.Select(p => new PlayerStateDto(
            p.Id,
            p.Villain,
            p.Power,
            p.Locations,
            p.VillainDeckCount,
            p.FateDeckCount
        )).ToList(),
        state.CurrentPlayerIndex,
        state.Turn
    );
}
