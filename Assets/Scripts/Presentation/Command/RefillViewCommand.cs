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
            int start = 0;
            for (int i = 1; i <= list.Count; i++)
            {
                bool runEnds = i == list.Count || list[i].y != list[i - 1].y + 1;
                if (!runEnds) continue;

                int count = i - start;
                for (int k = start; k < i; k++) _dropCells[list[k].id] = count;
                maxCells = Mathf.Max(maxCells, count);
                start = i;
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
