using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FallViewCommand : IViewCommand
{
    private const float MinDuration = 0.1f;
    
    private readonly List<(int pieceId, int toX, int toY, int fromY)> _moves;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    private readonly float _secondsPerCell;

    public float Duration => _duration;
    
    public FallViewCommand(List<(int, int, int, int)> moves, TurnPlayer player, float secondsPerCell)
    {
        _moves = moves;
        _player = player;
        _secondsPerCell = secondsPerCell;

        int maxCells = 0;
        foreach (var (_, _, toY, fromY) in moves)
        {
            int cells = Mathf.Abs(toY - fromY);
            if (cells > maxCells) maxCells = cells;
        }
        _duration = Mathf.Max(MinDuration, maxCells * secondsPerCell);
    }

    
    private float DistanceInCells(int pieceId, int toX, int toY)
    {
        PieceView view = _player.GetView(pieceId);
        if (view == null) return 0f;
        return Mathf.Abs(_player.CellToWorld(toX, toY).y - view.transform.position.y) / _player.CellSize;
    }
    
    public void Execute()
    {
        foreach (var (pieceId, toX, toY, fromY) in _moves)
        {
            PieceView view = _player.GetView(pieceId);
            if (view == null) continue;

            float duration = Mathf.Max(MinDuration, Mathf.Abs(toY - fromY) * _secondsPerCell);
            view.MoveTo(_player.CellToWorld(toX, toY), duration, Ease.InQuad, true);
        }
    }
}
