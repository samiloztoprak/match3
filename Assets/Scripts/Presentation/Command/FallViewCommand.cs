using System.Collections.Generic;
using DG.Tweening;

public class FallViewCommand : IViewCommand
{
    private readonly List<(int pieceId, int toX, int toY)> _moves;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public FallViewCommand(List<(int, int, int)> moves, TurnPlayer player, float duration)
    {
        _moves = moves;
        _player = player;
        _duration = duration;
    }

    public float Duration => _duration;
    
    public void Execute()
    {
        foreach (var (pieceId, toX, toY) in _moves)
        {
            PieceView view = _player.GetView(pieceId);
            view?.MoveTo(_player.CellToWorld(toX, toY), _duration, Ease.InQuad, true);
        }
    }
}
