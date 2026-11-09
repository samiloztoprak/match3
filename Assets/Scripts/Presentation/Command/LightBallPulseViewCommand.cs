using DG.Tweening;
using UnityEngine;

public class LightBallPulseViewCommand : IViewCommand
{
    private readonly int _pieceId;
    private readonly TurnPlayer _player;
    private readonly float _duration;

    public float Duration => _duration;

    public LightBallPulseViewCommand(int pieceId, TurnPlayer player, float duration)
    {
        _pieceId = pieceId;
        _player = player;
        _duration = duration;
    }

    public void Execute()
    {
        PieceView view = _player.GetView(_pieceId);
        if (view == null) return;

        Vector3 baseScale = view.transform.localScale;
        Sequence seq = DOTween.Sequence();
        seq.Append(view.transform.DOScale(baseScale * 1.4f, _duration * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(view.transform.DOScale(baseScale, _duration * 0.5f).SetEase(Ease.InQuad));
    }
}
