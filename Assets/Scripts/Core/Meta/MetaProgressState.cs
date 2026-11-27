using System;
using System.Collections.Generic;

[Serializable]
public sealed class MetaProgressState
{
    [Serializable]
    public struct BoosterBalance
    {
        public BoosterType Type;
        public int Count;
    }

    [Serializable]
    public struct LevelBest
    {
        public int LevelIndex;
        public int Stars;
    }

    public int Spins = 1;
    public int StarBank;
    public int TotalStars;
    public int PendingFly;
    public List<BoosterBalance> Boosters = new List<BoosterBalance>();
    public List<string> OwnedCards = new List<string>();
    public List<string> ClaimedSets = new List<string>();
    public List<LevelBest> LevelBests = new List<LevelBest>();

    public MetaProgressState Copy()
    {
        var copy = (MetaProgressState)MemberwiseClone();
        copy.Boosters = new List<BoosterBalance>(Boosters);
        copy.OwnedCards = new List<string>(OwnedCards);
        copy.ClaimedSets = new List<string>(ClaimedSets);
        copy.LevelBests = new List<LevelBest>(LevelBests);
        return copy;
    }

    public int GetBooster(BoosterType type)
    {
        foreach (BoosterBalance balance in Boosters)
        {
            if (balance.Type == type) return balance.Count;
        }
        return 3;
    }

    public void SetBooster(BoosterType type, int count)
    {
        var balance = new BoosterBalance { Type = type, Count = Math.Max(0, count) };
        for (int index = 0; index < Boosters.Count; index++)
        {
            if (Boosters[index].Type != type) continue;
            Boosters[index] = balance;
            return;
        }

        Boosters.Add(balance);
    }

    public void AddStars(int stars, int starsPerSpin, bool queueMenuFly)
    {
        if (stars <= 0) return;

        var (bank, spins) = WheelLogic.AddStars(StarBank, stars, starsPerSpin);
        StarBank = bank;
        Spins += spins;
        TotalStars += stars;
        if (queueMenuFly) PendingFly += stars;
    }

    public bool AddCard(string cardId, int starsPerSpin)
    {
        if (OwnedCards.Contains(cardId))
        {
            AddStars(1, starsPerSpin, false);
            return false;
        }

        OwnedCards.Add(cardId);
        return true;
    }
}
