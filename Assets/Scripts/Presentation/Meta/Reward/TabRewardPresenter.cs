using System;
using UnityEngine;

public class TabRewardPresenter : MonoBehaviour, IRewardPresenter
{
    [SerializeField] private WheelRewardType _type = WheelRewardType.Booster;
    [SerializeField] private MenuPage _targetPage = MenuPage.Home;
    [SerializeField] private RewardSpotlight _spotlight;
    [SerializeField] private RewardFlyer _flyer;
    [SerializeField] private MenuTabBar _tabBar;
    [SerializeField] private int _maxIcons = 5;

    public WheelRewardType Type => _type;

    private void OnValidate()
    {
        if (_spotlight == null) _spotlight = AutoRef.InScene<RewardSpotlight>();
        if (_flyer == null) _flyer = AutoRef.InScene<RewardFlyer>();
        if (_tabBar == null) _tabBar = AutoRef.InScene<MenuTabBar>();
    }

    public void Present(RewardContext context, Action onComplete)
    {
        WheelConfig.Segment reward = context.Reward;

        _spotlight.Show(reward.Icon, reward.AmountLabel, context.From, () =>
        {
            MenuTabButton target = _tabBar.GetButton(_targetPage);
            Vector3 from = _spotlight.IconPosition;
            _spotlight.Release();

            _flyer.Fly(reward.Icon, from, _spotlight.IconScale, target.Rect, Mathf.Clamp(reward.Amount, 1, _maxIcons),
                () =>
                {
                    target.Punch();
                    AudioManager.Play(SoundId.RewardArrive);
                },
                onComplete);
        });
    }
}