using System.Collections;
using UnityEngine;

public class MenuStarIntro : MonoBehaviour
{
    [SerializeField] private RewardFlyer _flyer;
    [SerializeField] private MenuTabBar _tabBar;
    [SerializeField] private Sprite _starSprite;
    [SerializeField] private float _startDelay = 0.4f;
    [SerializeField] private int _maxIcons = 5;

    private void OnValidate()
    {
        if (_flyer == null) _flyer = GetComponent<RewardFlyer>();
        if (_tabBar == null) _tabBar = AutoRef.InScene<MenuTabBar>();
    }

    private IEnumerator Start()
    {
        if (MetaProgress.State.PendingFly <= 0) yield break;

        while (LoadingScreen.IsLoading) yield return null;
        yield return new WaitForSecondsRealtime(_startDelay);

        MenuTabButton target = _tabBar == null ? null : _tabBar.GetButton(MenuPage.Wheel);
        if (target == null || _flyer == null || _starSprite == null) yield break;

        int stars = MetaProgress.State.PendingFly;
        if (stars <= 0) yield break;

        _flyer.transform.SetAsLastSibling();
        _flyer.Fly(_starSprite, _flyer.Center, target.Rect, Mathf.Min(stars, _maxIcons), target.Punch,
            () => WheelProgress.CompletePendingFly(stars), unscaledTime: true);
    }
}
