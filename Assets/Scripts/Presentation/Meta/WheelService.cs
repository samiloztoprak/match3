
public readonly struct WheelSpinResult
{
    public readonly int SegmentIndex;
    public readonly WheelConfig.Segment Reward;

    public WheelSpinResult(int segmentIndex, WheelConfig.Segment reward)
    {
        SegmentIndex = segmentIndex;
        Reward = reward;
    }
}

public static class WheelService
{
    public static bool TrySpin(WheelConfig config, IRandom random, out WheelSpinResult result)
    {
        result = default;
        if (!WheelProgress.TryUseSpin()) return false;

        int index = WheelLogic.PickSegment(config.Weights(), random);
        WheelConfig.Segment reward = config.Segments[index];

        switch (reward.Type)
        {
            case WheelRewardType.Stars:
                WheelProgress.AddStars(reward.Amount, config.StarsPerSpin, false);
                break;

            case WheelRewardType.Booster:
                BoosterInventory.Add(reward.Booster, reward.Amount);
                break;
            // CardPack
        }
        

        result = new WheelSpinResult(index, reward);
        return true;
    }
}