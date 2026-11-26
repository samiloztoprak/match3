using System;
using DG.Tweening;
using UnityEngine;

public class MenuTabBar : MonoBehaviour
{
    [Serializable]
    private struct PageRoot
    {
        public MenuPage Page;
        public RectTransform Root;
    }

    [SerializeField] private MenuTabButton[] _buttons;
    [SerializeField] private PageRoot[] _pages;
    [SerializeField] private MenuPage _defaultPage = MenuPage.Home;
    [SerializeField] private float _slideDuration = 0.3f;
    [SerializeField] private Ease _slideEase = Ease.OutCubic;

    public MenuPage Current { get; private set; }
    public bool Locked { get; set; }
    public event Action<MenuPage> PageChanged;

    private void OnValidate()
    {
        if (_buttons == null || _buttons.Length == 0) _buttons = GetComponentsInChildren<MenuTabButton>(true);
    }

    private void Awake()
    {
        Current = _defaultPage;
        foreach (PageRoot p in _pages)
        {
            if (p.Root == null) continue;
            p.Root.anchoredPosition = Vector2.zero;
            p.Root.gameObject.SetActive(p.Page == _defaultPage);
        }
        RefreshButtons();
    }
    
    public void OnDisable()
    {
        foreach (PageRoot p in _pages)
        {
            if (p.Root != null) p.Root.DOKill();
        }
    }

    public void Select(MenuPage page)
    {
        if (Locked || page == Current) return;
        
        RectTransform from = GetRoot(Current);
        RectTransform to = GetRoot(page);
        
        if (to == null) return;
        
        foreach (PageRoot p in _pages)
        {
            if (p.Root != null) p.Root.DOKill(true);
        }
        
        float width = to.rect.width;
        float dir = (int)page > (int)Current ? 1f : -1f;
        
        to.gameObject.SetActive(true);
        to.anchoredPosition = new Vector2(width * dir, 0f);
        to.DOAnchorPos(Vector2.zero, _slideDuration).SetEase(_slideEase);

        
        if (from != null)
        {
            from.DOAnchorPos(new Vector2(-width * dir, 0f), _slideDuration)
                .SetEase(_slideEase)
                .OnComplete(() =>
                {
                    from.gameObject.SetActive(false);
                    from.anchoredPosition = Vector2.zero;
                });
        }
        
        Current = page;
        RefreshButtons();
        PageChanged?.Invoke(page);
    }

    private void RefreshButtons()
    {
        foreach (MenuTabButton button in _buttons)
        {
            button.SetSelected(button.Page == Current);
        }
    }
    
    private RectTransform GetRoot(MenuPage page)
    {
        foreach (PageRoot p in _pages)
        {
            if (p.Page == page) return p.Root;
        }
        return null;
    }
    
    public MenuTabButton GetButton(MenuPage page)
    {
        foreach (MenuTabButton button in _buttons)
        {
            if (button.Page == page) return button;
        }
        return null;
    }
}