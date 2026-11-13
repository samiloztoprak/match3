using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class PopupView : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private RectTransform _window;
    [SerializeField] private float _dimAlpha = 1f;

    public bool IsOpen { get; private set; }

    protected virtual void OnValidate()
    {
        if (_group == null) _group = GetComponent<CanvasGroup>();
        if (_window == null) _window = AutoRef.Child<RectTransform>(this, "ui_panel_window");
    }

    protected virtual void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        IsOpen = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        _group.DOKill();
        _window.DOKill();
        _group.alpha = 0f;
        _group.interactable = false;
        _window.localScale = Vector3.one * 0.6f;

        Sequence seq = DOTween.Sequence().SetTarget(_window);
        seq.Append(_group.DOFade(_dimAlpha, 0.2f));
        seq.Join(_window.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.OnComplete(() =>
        {
            _group.interactable = true;
            OnOpened();
        });
    }

    public void Close(Action onClosed = null)
    {
        IsOpen = false;
        _group.interactable = false;
        _group.DOKill();
        _window.DOKill();

        Sequence seq = DOTween.Sequence().SetTarget(_window);
        seq.Append(_window.DOScale(0.8f, 0.15f).SetEase(Ease.InBack));
        seq.Join(_group.DOFade(0f, 0.15f));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onClosed?.Invoke();
        });
    }

    protected virtual void OnOpened() { }
}