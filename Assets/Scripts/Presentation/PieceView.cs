using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Piece _piece;
    public Piece Piece => _piece;
    
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(Piece piece, PieceVisualConfig visualConfig)
    {
        _piece = piece;
        RefreshSprite(visualConfig);
    }

    public void RefreshSprite(PieceVisualConfig visualConfig)
    {
        _spriteRenderer.sprite = visualConfig.GetSprite(_piece);
    }

    public void SetSortingOrder(int order)
    {
        _spriteRenderer.sortingOrder = order;
    }
    
    public void MoveTo(Vector3 targetPos, float duration, Ease ease = Ease.OutQuad, bool bounceOnLand = false)
    {
        Tween moveTween = transform.DOMove(targetPos, duration).SetEase(ease);

        if (bounceOnLand)
        {
            moveTween.OnComplete((() =>
            {
                transform.DOPunchPosition(Vector3.up * 0.15f, 0.25f, 1, 0.3f);
            }));
        }
    }

    public void ShrinkAndRelease(float duration, System.Action<PieceView> onComplete)
    {
        transform.DOScale(Vector3.zero, duration).OnComplete(() => onComplete(this));
    }
}
