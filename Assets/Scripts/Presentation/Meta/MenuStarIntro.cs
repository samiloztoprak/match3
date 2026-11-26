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
        int stars = WheelProgress.ConsumePendingFly();
        if (stars <= 0) yield break;

        yield return new WaitForSeconds(_startDelay);

        MenuTabButton target = _tabBar.GetButton(MenuPage.Wheel);
        _flyer.Fly(_starSprite, _flyer.Center, target.Rect, Mathf.Min(stars, _maxIcons), target.Punch, null);
    }
}