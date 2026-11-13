using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BoosterButtonView : MonoBehaviour
{
    [SerializeField] private BoosterType _type;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _countBadge;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _selectionRing;

    private Tween _ringTween;

    public BoosterType Type => _type;
    public event Action<BoosterType> Clicked;

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_countBadge == null) _countBadge = FindChild("ui_image_badge_count")?.gameObject;
        if (_countText == null) _countText = FindChild("ui_text_count_value")?.GetComponent<TMP_Text>();
        if (_selectionRing == null) _selectionRing = FindChild("ui_image_selection_ring")?.GetComponent<Image>();

        string lower = name.ToLowerInvariant();
        if (lower.Contains("hammer")) _type = BoosterType.Hammer;
        else if (lower.Contains("row")) _type = BoosterType.Row;
        else if (lower.Contains("column")) _type = BoosterType.Column;
        else if (lower.Contains("shuffle")) _type = BoosterType.Shuffle;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
        SetSelected(false);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
        _ringTween?.Kill();
    }

    public void SetCount(int count)
    {
        _countBadge.SetActive(true);
        _countText.text = count > 0 ? count.ToString() : "+";
    }

    public void SetSelected(bool selected)
    {
        _ringTween?.Kill();
        _selectionRing.gameObject.SetActive(selected);
        transform.localScale = Vector3.one;

        if (selected)
        {
            Transform ring = _selectionRing.transform;
            ring.localScale = Vector3.one;
            _ringTween = ring.DOScale(1.12f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 6, 0.5f);
        }
    }

    public void PlayDenied()
    {
        transform.DOKill(true);
        transform.DOShakePosition(0.3f, new Vector3(12f, 0f, 0f), 20, 0f);
    }

    private void OnClick()
    {
        Clicked?.Invoke(_type);
    }

    private Transform FindChild(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }
}