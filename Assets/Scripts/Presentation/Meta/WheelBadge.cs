using TMPro;
using UnityEngine;

public class WheelBadge : MonoBehaviour
{
    [SerializeField] private GameObject _badge;
    [SerializeField] private TMP_Text _text;

    private void OnValidate()
    {
        if (_badge == null) _badge = AutoRef.Child<Transform>(this, "ui_badge_spins")?.gameObject;
        if (_text == null) _text = AutoRef.Child<TMP_Text>(this, "ui_text_badge");
    }

    private void OnEnable()
    {
        WheelProgress.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        WheelProgress.Changed -= Refresh;
    }

    private void Refresh()
    {
        int spins = WheelProgress.Spins;
        _badge.SetActive(spins > 0);
        _text.text = "x" + spins;
    }
}