using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Piece _piece;
    public Piece Piece => _piece;
    private Vector3 _baseScale;
    private Tween _hintTween;
    private Vector3 _preHintScale;
    
    
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
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

    public void SetSizeInCells(int width, int height)
    {
        transform.localScale = new Vector3(_baseScale.x * width, _baseScale.y * height, _baseScale.z);
    }
    
    public void MoveTo(Vector3 targetPos, float duration, Ease ease = Ease.OutQuad, bool bounceOnLand = false)
    {
        transform.DOKill();
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
        transform.DOKill();
        transform.DOScale(Vector3.zero, duration).OnComplete(() => onComplete(this));
    }
    
    public void RefreshSprite(PieceVisualConfig visualConfig, int healthOverride)
    {
        _spriteRenderer.sprite = visualConfig.GetSprite(_piece, healthOverride);
    }
    
    public void PlayHintPulse()
    {
        if (_hintTween != null)
        {
            return;
        }
        _preHintScale = transform.localScale;
        _hintTween = transform.DOScale(_preHintScale * 1.15f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void StopHintPulse()
    {
        if (_hintTween == null)
        {
            return;
        }
        _hintTween.Kill();
        _hintTween = null;
        transform.localScale = _preHintScale;
    }
}
