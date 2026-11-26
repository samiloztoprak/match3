using TMPro;
using UnityEngine;

public class StarCounterView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private void OnValidate()
    {
        if (_text == null)
        {
            _text = GetComponentInChildren<TMP_Text>(true);
        }
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
        _text.text = WheelProgress.TotalStars.ToString();
    }
}