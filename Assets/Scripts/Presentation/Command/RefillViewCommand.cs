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
    private readonly Dictionary<int, int> _dropCells = new Dictionary<int, int>();
    private readonly float _duration;

    public float Duration => _duration;
    
    public RefillViewCommand(List<(int, PieceColor, int, int)> moves, TurnPlayer player, float secondsPerCell)
    {
        _moves = moves;
        _player = player;
        _secondsPerCell = secondsPerCell;

        var byColumn = new Dictionary<int, List<(int id, int y)>>();
        foreach (var (id, _, toX, toY) in moves)
        {
            if (!byColumn.TryGetValue(toX, out var list))
            {
                list = new List<(int, int)>();
                byColumn[toX] = list;
            }
            list.Add((id, toY));
        }

        int maxCells = 0;
        foreach (var list in byColumn.Values)
        {
            list.Sort((a, b) => a.y.CompareTo(b.y));
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                int spawnY = i - count;
                int cells = list[i].y - spawnY;
                _dropCells[list[i].id] = cells;
                maxCells = Mathf.Max(maxCells, cells);
            }
        }
        _duration = Mathf.Max(MinDuration, maxCells * secondsPerCell);
    }


    public void Execute()
    {
        foreach (var (pieceId, color, toX, toY) in _moves)
        {
            int cells = _dropCells[pieceId];
            Vector3 spawnPos = _player.CellToWorld(toX, toY - cells);
            PieceView view = _player.SpawnPieceView(new ColoredPiece(pieceId, color), spawnPos);

            float duration = Mathf.Max(MinDuration, cells * _secondsPerCell);
            view.MoveTo(_player.CellToWorld(toX, toY), duration, Ease.InQuad, true);
        }
    }
}
