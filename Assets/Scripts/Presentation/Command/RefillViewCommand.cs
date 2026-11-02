using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RefillViewCommand : IViewCommand
{
    private readonly List<(int pieceId, PieceColor color, int toX, int toY)> _moves;
    private readonly TurnPlayer _player;
    private readonly float _duration;

    public RefillViewCommand(List<(int, PieceColor, int, int)> moves, TurnPlayer player, float duration)
    {
        _moves = moves;
        _player = player;
        _duration = duration;
    }
    
    public float Duration => _duration;
    
    public void Execute()
    {
        var totalPerColumn = new Dictionary<int, int>();
        foreach (var (_, _, toX, _) in _moves)
        {
            totalPerColumn.TryGetValue(toX, out int count);
            totalPerColumn[toX] = count + 1;
        }

        foreach (var (pieceId, color, toX, toY) in _moves)
        {
            Vector3 spawnPos = _player.CellToWorld(toX, toY - totalPerColumn[toX]);
            PieceView view = _player.SpawnPieceView(new ColoredPiece(pieceId, color), spawnPos);
            view.MoveTo(_player.CellToWorld(toX, toY), _duration, Ease.InQuad, true);
        }
    }
}
