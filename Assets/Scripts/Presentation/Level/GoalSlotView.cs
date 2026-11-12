using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalSlotView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _count;
    [SerializeField] private GameObject _check;

    private int _shown;
    private int _pending;

    public GoalType Type { get; private set; }
    public PieceColor Color { get; private set; }
    public RectTransform Target => (RectTransform)_icon.transform;
    public bool HasPending => _pending > 0;

    public void Setup(LevelGoal goal, Sprite icon, int remaining)
    {
        Type = goal.Type;
        Color = goal.Color;
        _icon.sprite = icon;
        _icon.enabled = icon != null;
        _shown = remaining;
        _pending = remaining;
        Refresh();
    }

    public bool Matches(Piece piece)
    {
        if (piece == null)
        {
            return false;
        }
        if (Type == GoalType.CollectColor)
        {
            return piece.VisualType == PieceVisualType.Standard && piece.Color == Color;
        }
        return piece.GoalKind == Type;
    }

    public bool TryReserve()
    {
        if (_pending <= 0)
        {
            return false;
        }
        _pending--;
        return true;
    }

    public void OnIconArrived()
    {
        _shown = Mathf.Max(0, _shown - 1);
        Refresh();

        Transform t = _icon.transform;
        t.DOKill(true);
        t.localScale = Vector3.one;
        t.DOPunchScale(Vector3.one * 0.25f, 0.2f, 6, 0.5f);
    }

    public void SyncTo(int remaining)
    {
        _shown = remaining;
        _pending = Mathf.Min(_pending, remaining);
        Refresh();
    }

    private void Refresh()
    {
        _count.text = _shown.ToString();
        _count.gameObject.SetActive(_shown > 0);
        if (_check != null)
        {
            _check.SetActive(_shown == 0);
        }
    }
}