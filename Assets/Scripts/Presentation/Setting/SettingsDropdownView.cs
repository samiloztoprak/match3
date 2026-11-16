using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDropdownView : MonoBehaviour
{
    [SerializeField] private Button _toggleButton;
    [SerializeField] private RectTransform _gearIcon;
    [SerializeField] private RectTransform _itemsRoot;
    [SerializeField] private Button _blocker;
    [SerializeField] private float _duration = 0.25f;
    [SerializeField] private float _stagger = 0.04f;

    private readonly List<RectTransform> _items = new List<RectTransform>();
    private readonly List<Vector3> _openPositions = new List<Vector3>();

    public bool IsOpen { get; private set; }

    private void OnValidate()
    {
        if (_toggleButton == null) _toggleButton = AutoRef.Child<Button>(this, "ui_button_settings");
        if (_gearIcon == null) _gearIcon = AutoRef.Child<RectTransform>(this, "ui_image_gear");
        if (_itemsRoot == null) _itemsRoot = AutoRef.Child<RectTransform>(this, "ui_settings_items");
        if (_blocker == null) _blocker = AutoRef.Child<Button>(this, "ui_button_blocker");
    }

    private void Awake()
    {
        foreach (RectTransform item in _itemsRoot)
        {
            _items.Add(item);
            _openPositions.Add(item.localPosition);
        }

        CloseImmediately();
    }

    private void OnEnable()
    {
        _toggleButton.onClick.AddListener(Toggle);
        _blocker.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        _toggleButton.onClick.RemoveListener(Toggle);
        _blocker.onClick.RemoveListener(Close);
        KillTweens();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        KillTweens();

        _blocker.gameObject.SetActive(true);
        _gearIcon.DORotate(new Vector3(0f, 0f, -90f), _duration).SetEase(Ease.OutBack);

        Vector3 collapsed = CollapsedPosition();
        for (int i = 0; i < _items.Count; i++)
        {
            RectTransform item = _items[i];
            item.gameObject.SetActive(true);
            item.localPosition = collapsed;
            item.localScale = Vector3.one * 0.4f;

            float delay = i * _stagger;
            item.DOLocalMove(_openPositions[i], _duration).SetDelay(delay).SetEase(Ease.OutBack);
            item.DOScale(1f, _duration).SetDelay(delay).SetEase(Ease.OutBack);
        }
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        KillTweens();

        _blocker.gameObject.SetActive(false);
        _gearIcon.DORotate(Vector3.zero, _duration).SetEase(Ease.OutQuad);

        Vector3 collapsed = CollapsedPosition();
        int last = _items.Count - 1;
        for (int i = 0; i <= last; i++)
        {
            RectTransform item = _items[i];
            float delay = (last - i) * _stagger;   // the farthest item goes back first
            item.DOLocalMove(collapsed, _duration * 0.8f).SetDelay(delay).SetEase(Ease.InBack);
            item.DOScale(0.4f, _duration * 0.8f).SetDelay(delay).SetEase(Ease.InBack)
                .OnComplete(() => item.gameObject.SetActive(false));
        }
    }

    private void CloseImmediately()
    {
        IsOpen = false;
        _blocker.gameObject.SetActive(false);
        _gearIcon.localRotation = Quaternion.identity;
        foreach (RectTransform item in _items)
        {
            item.gameObject.SetActive(false);
        }
    }

    private Vector3 CollapsedPosition()
    {
        return _itemsRoot.InverseTransformPoint(_toggleButton.transform.position);
    }

    private void KillTweens()
    {
        _gearIcon.DOKill();
        foreach (RectTransform item in _items)
        {
            item.DOKill();
        }
    }
}