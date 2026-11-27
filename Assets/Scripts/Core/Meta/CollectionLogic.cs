using System;
using System.Collections.Generic;

public static class CollectionLogic
{
    public const int CardsPerPack = 3;
    public const int BoostersPerSet = 3;

    public static int[] DrawPack(IReadOnlyList<int> weights, IRandom random)
    {
        var remaining = new int[weights.Count];
        int eligible = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            remaining[i] = weights[i];
            if (weights[i] > 0) eligible++;
        }

        if (eligible < CardsPerPack)
            throw new ArgumentException("A pack requires at least three eligible cards.", nameof(weights));

        var drawn = new int[CardsPerPack];
        for (int i = 0; i < drawn.Length; i++)
        {
            int index = WheelLogic.PickSegment(remaining, random);
            drawn[i] = index;
            remaining[index] = 0;
        }

        return drawn;
    }

    public static bool IsComplete(IReadOnlyList<string> cards, List<string> owned)
    {
        if (cards.Count == 0) return false;
        foreach (string card in cards)
        {
            if (!owned.Contains(card)) return false;
        }
        return true;
    }

    public static bool TryClaimSet(MetaProgressState state, string setId, IReadOnlyList<string> cards)
    {
        if (state.ClaimedSets.Contains(setId) || !IsComplete(cards, state.OwnedCards))
            return false;

        foreach (BoosterType booster in Enum.GetValues(typeof(BoosterType)))
            state.SetBooster(booster, state.GetBooster(booster) + BoostersPerSet);

        state.ClaimedSets.Add(setId);
        return true;
    }
}
