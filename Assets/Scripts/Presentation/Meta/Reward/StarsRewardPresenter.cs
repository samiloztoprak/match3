using System;
using UnityEngine;

public class StarsRewardPresenter : MonoBehaviour, IRewardPresenter
{
    [SerializeField] private RewardSpotlight _spotlight;
    [SerializeField] private RewardFlyer _flyer;
    [SerializeField] private WheelProgressBar _bar;
    [SerializeField] private int _maxIcons = 5;

    public WheelRewardType Type => WheelRewardType.Stars;

    private void OnValidate()
    {
        if (_spotlight == null) _spotlight = AutoRef.InScene<RewardSpotlight>();
        if (_flyer == null) _flyer = AutoRef.InScene<RewardFlyer>();
        if (_bar == null) _bar = AutoRef.InScene<WheelProgressBar>();
    }

    public void Present(RewardContext context, Action onComplete)
    {
        WheelConfig.Segment reward = context.Reward;

        _spotlight.Show(reward.Icon, reward.AmountLabel, context.From, () =>
        {
            Vector3 from = _spotlight.IconPosition;
            _spotlight.Release();

            _flyer.Fly(reward.Icon, from, _spotlight.IconScale, _bar.Rect, Mathf.Clamp(reward.Amount, 1, _maxIcons),
                () =>
                {
                    _bar.Punch();
                    AudioManager.Play(SoundId.StarPop);
                },
                onComplete);
        });
    }
}