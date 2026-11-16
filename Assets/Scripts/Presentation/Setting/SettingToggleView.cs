using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingToggleView : MonoBehaviour
{
    [SerializeField] private SettingType _type;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _offMark;

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_offMark == null) _offMark = AutoRef.Child<Transform>(this, "ui_image_off")?.gameObject;

        string lower = name.ToLowerInvariant();
        if (lower.Contains("music")) _type = SettingType.Music;
        else if (lower.Contains("sound")) _type = SettingType.Sound;
        else if (lower.Contains("vibration")) _type = SettingType.Vibration;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
        GameSettings.Changed += OnChanged;
        Refresh();
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
        GameSettings.Changed -= OnChanged;
        transform.DOKill(true);
    }

    private void OnClick()
    {
        GameSettings.Toggle(_type);
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 6, 0.5f);
    }

    private void OnChanged(SettingType type, bool on)
    {
        if (type == _type) Refresh();
    }

    private void Refresh()
    {
        _offMark.SetActive(!GameSettings.Get(_type));
    }
}