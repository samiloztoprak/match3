using DG.Tweening;

public class TntChargeViewCommand : IViewCommand
{
    private readonly int _pieceId;
    private readonly TurnPlayer _player;
    private readonly float _duration;

    public TntChargeViewCommand(int pieceId, TurnPlayer player, float duration)
    {
        _pieceId = pieceId; _player = player; _duration = duration;
    }

    public float Duration => _duration;
    public void Execute()
    {
        PieceView view = _player.GetView(_pieceId);
        if (view == null) return;
        view.transform.DOScale(view.transform.localScale * 1.3f, _duration).SetEase(Ease.OutQuad);
    }
}