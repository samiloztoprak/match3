using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSpotlight : MonoBehaviour
{
    [SerializeField] private RectTransform _root;
    [SerializeField] private Image _glow;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amount;
    [SerializeField] private Vector2 _centerOffset = new Vector2(0f, 120f);
    [SerializeField] private float _riseDuration = 0.45f;
    [SerializeField] private float _iconScale = 1.6f;
    [SerializeField] private float _holdDuration = 0.7f;
    [SerializeField] private float _glowTurnSeconds = 4f;
    [SerializeField] private float _glowFadeOut = 0.25f;

    public Vector3 IconPosition => _icon.rectTransform.position;
    public float IconScale => _iconScale;

    private void OnValidate()
    {
        if (_root == null) _root = (RectTransform)transform;
        if (_glow == null) _glow = AutoRef.Child<Image>(this, "ui_spotlight_glow");
        if (_icon == null) _icon = AutoRef.Child<Image>(this, "ui_spotlight_icon");
        if (_amount == null) _amount = AutoRef.Child<TMP_Text>(this, "ui_spotlight_amount");
    }

    private void OnDisable()
    {
        Kill();
    }

    public void Show(Sprite icon, string amount, Vector3 from, Action onHoldEnd)
    {
        gameObject.SetActive(true);
        Kill();

        Vector3 center = _root.TransformPoint(_root.rect.center + _centerOffset);
        RectTransform glow = _glow.rectTransform;
        RectTransform iconRt = _icon.rectTransform;

        glow.position = center;
        glow.localRotation = Quaternion.identity;
        glow.localScale = Vector3.one * 0.6f;
        _glow.color = new Color(_glow.color.r, _glow.color.g, _glow.color.b, 0f);

        _icon.sprite = icon;
        _amount.text = amount;
        _icon.gameObject.SetActive(true);
        iconRt.position = from;
        iconRt.localScale = Vector3.one * 0.6f;

        glow.DORotate(new Vector3(0f, 0f, -360f), _glowTurnSeconds, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        DOTween.Sequence().SetTarget(this)
            .Append(iconRt.DOMove(center, _riseDuration).SetEase(Ease.OutCubic))
            .Join(iconRt.DOScale(_iconScale, _riseDuration).SetEase(Ease.OutBack))
            .Join(_glow.DOFade(1f, _riseDuration))
            .Join(glow.DOScale(1f, _riseDuration).SetEase(Ease.OutBack))
            .AppendCallback(() =>
            {
                AudioManager.Play(SoundId.RewardReveal);
                Haptics.Vibrate();
            })
            .AppendInterval(_holdDuration)
            .OnComplete(() => onHoldEnd?.Invoke());
    }

    public void Release()
    {
        DOTween.Kill(this);
        _icon.gameObject.SetActive(false);
        _glow.DOFade(0f, _glowFadeOut)
            .SetTarget(this)
            .OnComplete(() =>
            {
                _glow.rectTransform.DOKill();
                gameObject.SetActive(false);
            });
    }

    private void Kill()
    {
        DOTween.Kill(this);
        _glow.rectTransform.DOKill();
        _icon.rectTransform.DOKill();
    }
}