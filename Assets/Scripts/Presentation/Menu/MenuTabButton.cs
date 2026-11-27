using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuTabButton : MonoBehaviour
{
    [SerializeField] private MenuPage _page;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _selectedVisual;
    [SerializeField] private GameObject _lockVisual;
    [SerializeField] private MenuTabBar _bar;
    
    public RectTransform Rect => (RectTransform)transform;
    
    public MenuPage Page => _page;
    public bool IsLocked => _lockVisual != null && _lockVisual.activeSelf;

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_bar == null) _bar = GetComponentInParent<MenuTabBar>();
        if (_selectedVisual == null) _selectedVisual = FindChild("ui_image_selected");
        if (_lockVisual == null) _lockVisual = FindChild("ui_image_lock");

        string lower = name.ToLowerInvariant();
        if (lower.Contains("collection")) _page = MenuPage.Collection;
        else if (lower.Contains("home")) _page = MenuPage.Home;
        else if (lower.Contains("wheel")) _page = MenuPage.Wheel;
    }

    private void OnEnable() => _button.onClick.AddListener(OnClick);

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (IsLocked)
        {
            _lockVisual.transform.DOKill(true);
            _lockVisual.transform.DOPunchRotation(new Vector3(0f, 0f, 15f), 0.35f, 10, 1f);
            return;
        }
        _bar.Select(_page);
    }

    public void SetSelected(bool selected)
    {
        if (_selectedVisual != null) _selectedVisual.SetActive(selected);
    }

    public void SetLocked(bool locked)
    {
        if (_lockVisual != null) _lockVisual.SetActive(locked);
    }

    private GameObject FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName) return t.gameObject;
        }
        return null;
    }
    
    public void Punch()
    {
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 8, 0.5f);
    }
}
