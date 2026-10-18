using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private int _pieceId;
    public int PieceId => _pieceId;
    
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(Piece piece, PieceVisualConfig visualConfig)
    {
        _pieceId = piece.Id;
        _spriteRenderer.sprite = visualConfig.GetSprite(piece.Color);
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
