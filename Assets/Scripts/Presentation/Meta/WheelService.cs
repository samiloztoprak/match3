using System.Collections.Generic;

public readonly struct CardPackResult
{
    public readonly struct Entry
    {
        public readonly CollectionCatalog.Card Card;
        public readonly bool IsDuplicate;

        public Entry(CollectionCatalog.Card card, bool isDuplicate)
        {
            Card = card;
            IsDuplicate = isDuplicate;
        }
    }

    public readonly Entry[] Cards;
    public readonly int DuplicateStars;
    public readonly IReadOnlyList<string> CompletedSets;

    public CardPackResult(Entry[] cards, int duplicateStars, IReadOnlyList<string> completedSets)
    {
        Cards = cards;
        DuplicateStars = duplicateStars;
        CompletedSets = completedSets;
    }
}

public readonly struct WheelSpinResult
{
    public readonly int SegmentIndex;
    public readonly WheelConfig.Segment Reward;
    public readonly CardPackResult Pack;

    public WheelSpinResult(int segmentIndex, WheelConfig.Segment reward, CardPackResult pack)
    {
        SegmentIndex = segmentIndex;
        Reward = reward;
        Pack = pack;
    }
}

public static class WheelService
{
    public static WheelConfig.Segment RewardAt(WheelConfig config, int index)
    {
        var reward = config.Segments[index];
        if (reward.Type == WheelRewardType.CardPack
            && (config.Collection == null || !config.Collection.HasIncompleteSet(MetaProgress.State)))
        {
            reward.Type = WheelRewardType.Stars;
            reward.Amount = 5;
            reward.Icon = config.FallbackStarIcon;
        }
        return reward;
    }

    public static bool TrySpin(WheelConfig config, IRandom random, out WheelSpinResult result)
    {
        result = default;
        if (WheelProgress.Spins <= 0 || config.Segments.Length == 0) return false;

        var weights = config.Weights();
        int index = WheelLogic.PickSegment(weights, random);
        if (weights[index] <= 0) return false;

        var reward = RewardAt(config, index);
        if (reward.Amount <= 0) return false;

        MetaProgressState next = MetaProgress.BeginUpdate();
        next.Spins--;
        CardPackResult pack = default;
        switch (reward.Type)
        {
            case WheelRewardType.Stars:
                next.AddStars(reward.Amount, config.StarsPerSpin, false);
                break;
            case WheelRewardType.Booster:
                next.SetBooster(reward.Booster, next.GetBooster(reward.Booster) + reward.Amount);
                break;
            case WheelRewardType.CardPack:
                pack = OpenPack(config, next, random);
                break;
            default:
                return false;
        }

        MetaProgress.Commit(next);
        result = new WheelSpinResult(index, reward, pack);
        return true;
    }

    private static CardPackResult OpenPack(WheelConfig config, MetaProgressState next, IRandom random)
    {
        var cards = config.Collection.Draw(random);
        var entries = new CardPackResult.Entry[cards.Length];
        int duplicateStars = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            bool duplicate = !next.AddCard(cards[i].Id, config.StarsPerSpin);
            entries[i] = new CardPackResult.Entry(cards[i], duplicate);
            if (duplicate) duplicateStars++;
        }

        var completed = new List<string>();
        foreach (var set in config.Collection.Sets)
        {
            if (CollectionLogic.TryClaimSet(next, set.Id, set.CardIds())) completed.Add(set.Title);
        }

        return new CardPackResult(entries, duplicateStars, completed);
    }
}
