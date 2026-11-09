using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RefillViewCommand : IViewCommand
{
    private const float MinDuration = 0.1f;
    
    private readonly List<(int pieceId, PieceColor color, int toX, int toY)> _moves;
    private readonly TurnPlayer _player;
    private readonly float _secondsPerCell;
    private readonly Dictionary<int, int> _totalPerColumn;
    private readonly float _duration;

    public float Duration => _duration;
    
    public RefillViewCommand(List<(int, PieceColor, int, int)> moves, TurnPlayer player, float secondsPerCell)
    {
        _moves = moves;
        _player = player;
        _secondsPerCell = secondsPerCell;

        _totalPerColumn = new Dictionary<int, int>();
        foreach (var (_, _, toX, _) in moves)
        {
            _totalPerColumn.TryGetValue(toX, out int count);
            _totalPerColumn[toX] = count + 1;
        }

        float maxCells = 0f;
        foreach (int cells in _totalPerColumn.Values)
        {
            if (cells > maxCells) maxCells = cells;
        }
        _duration = Mathf.Max(MinDuration, maxCells * secondsPerCell);
    }


    public void Execute()
    {
        foreach (var (pieceId, color, toX, toY) in _moves)
        {
            int cells = _totalPerColumn[toX];
            Vector3 spawnPos = _player.CellToWorld(toX, toY - cells);
            PieceView view = _player.SpawnPieceView(new ColoredPiece(pieceId, color), spawnPos);

            float duration = Mathf.Max(MinDuration, cells * _secondsPerCell);
            view.MoveTo(_player.CellToWorld(toX, toY), duration, Ease.InQuad, true);
        }
    }
}
