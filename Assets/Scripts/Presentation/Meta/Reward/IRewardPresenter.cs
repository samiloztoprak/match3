using System;
using UnityEngine;

public readonly struct RewardContext
{
    public readonly WheelConfig.Segment Reward;
    public readonly Vector3 From;

    public RewardContext(WheelConfig.Segment reward, Vector3 from)
    {
        Reward = reward;
        From = from;
    }
}

public interface IRewardPresenter
{
    WheelRewardType Type { get; }
    void Present(RewardContext context, Action onComplete);
}