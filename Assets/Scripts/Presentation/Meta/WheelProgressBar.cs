using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelProgressBar : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _fillDuration = 0.4f;

    private int _shownBank = -1;

    public RectTransform Rect => (RectTransform)transform;

    private void OnValidate()
    {
        if (_fill == null) _fill = AutoRef.Child<Image>(this, "ui_progress_fill");
        if (_text == null) _text = AutoRef.Child<TMP_Text>(this, "ui_text_progress");
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        transform.DOKill(true);
    }

    public void Show(int bank, int perSpin, bool animate)
    {
        float target = perSpin > 0 ? (float)bank / perSpin : 0f;
        _text.text = bank + "/" + perSpin;
        DOTween.Kill(this);

        if (!animate || _shownBank < 0)
        {
            _fill.fillAmount = target;
        }
        else if (bank < _shownBank)
        {
            DOTween.Sequence().SetTarget(this)
                .Append(_fill.DOFillAmount(1f, _fillDuration).SetEase(Ease.OutCubic))
                .AppendCallback(() =>
                {
                    _fill.fillAmount = 0f;
                    Punch();
                })
                .Append(_fill.DOFillAmount(target, _fillDuration).SetEase(Ease.OutCubic));
        }
        else
        {
            _fill.DOFillAmount(target, _fillDuration).SetEase(Ease.OutCubic).SetTarget(this);
        }

        _shownBank = bank;
    }

    public void Punch()
    {
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.08f, 0.25f, 8, 0.5f);
    }
}