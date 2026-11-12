using System;
using System.Collections.Generic;

public class GoalTracker : ITargetPriority
{
    private readonly List<LevelGoal> _goals;
    private readonly int[] _remaining;

    public event Action OnProgressChanged;
    public event Action OnAllGoalsComplete;
    
    public GoalTracker(Board board, List<LevelGoal> goals)
    {
        _goals = new List<LevelGoal>(goals);

        var authoredTypes = new HashSet<GoalType>();
        foreach (var goal in _goals)
        {
            authoredTypes.Add(goal.Type);
        }

        foreach (var kvp in CountGoalKinds(board))
        {
            if (authoredTypes.Contains(kvp.Key))
            {
                continue;
            }
            _goals.Add(new LevelGoal { Type = kvp.Key, TargetCount = kvp.Value });
        }

        _remaining = new int[_goals.Count];
        for (int i = 0; i < _goals.Count; i++)
        {
            _remaining[i] = _goals[i].TargetCount;
        }
    }
    
    private static Dictionary<GoalType, int> CountGoalKinds(Board board)
    {
        var counts = new Dictionary<GoalType, int>();
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                IncrementCount(counts, board.GetCell(x, y)?.GoalKind);
                IncrementCount(counts, board.GetFloor(x, y)?.GoalKind);
            }
        }
        foreach (var overlay in board.OverlayPieces)
        {
            IncrementCount(counts, overlay.GoalKind);
        }
        return counts;
    }
    
    private static void IncrementCount(Dictionary<GoalType, int> counts, GoalType? kind)
    {
        if (kind == null) return;
        counts.TryGetValue(kind.Value, out int count);
        counts[kind.Value] = count + 1;
    }
    
    public IReadOnlyList<LevelGoal> Goals => _goals;
    public int GetRemaining(int index)
    {
        return _remaining[index];
    }

    public bool IsComplete()
    {
        return CountIncomplete() == 0;
    }

    public void ApplyProgress(GoalProgressData progress)
    {
        bool changed = false;

        for (int i = 0; i < _goals.Count; i++)
        {
            if (_remaining[i] <= 0)
            {
                continue;
            }

            int consumed = 0;
            if (_goals[i].Type == GoalType.CollectColor)
            {
                foreach (var color in progress.ClearedColors)
                {
                    if (color == _goals[i].Color)
                    {
                        consumed++;
                    }
                }
            }
            else
            {
                consumed = progress.GetCount(_goals[i].Type);
            }

            if (consumed > 0)
            {
                _remaining[i] = Math.Max(0, _remaining[i] - consumed);
                changed = true;
            }
        }

        if (changed)
        {
            OnProgressChanged?.Invoke();
            if (IsComplete())
            {
                OnAllGoalsComplete?.Invoke();
            }
        }
    }
    
    private int CountIncomplete()
    {
        int count = 0;
        for (int i = 0; i < _remaining.Length; i++)
        {
            if (_remaining[i] > 0)
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetPriority(Board board, int x, int y)
    {
        Piece piece = board.GetCell(x, y);

        if (piece != null && piece.GoalKind.HasValue && IsGoalActive(piece.GoalKind.Value, null))
        {
            return 2;
        }

        Piece above = y > 0 ? board.GetCell(x, y - 1) : null;
        if (piece != null && !piece.Flags.HasFlag(PieceFlags.Void)
                          && above != null && above.Flags.HasFlag(PieceFlags.Collectable) && !above.Flags.HasFlag(PieceFlags.Locked)
                          && above.GoalKind.HasValue && IsGoalActive(above.GoalKind.Value, null))
        {
            return 3;
        }
        
        OverlayPiece overlay = board.GetOverlayPieceAt(x, y);
        if (overlay != null && overlay.GoalKind.HasValue && IsGoalActive(overlay.GoalKind.Value, null))
        {
            bool specialOnly = overlay.Flags.HasFlag(PieceFlags.DamageableBySpecial) && !overlay.Flags.HasFlag(PieceFlags.DamageableByNeighbor);
            return specialOnly ? 3 : 2;
        }

        FloorPiece floor = board.GetFloor(x, y);
        if (floor != null && floor.GoalKind.HasValue && IsGoalActive(floor.GoalKind.Value, null))
        {
            return 2;
        }
        
        if (piece != null && piece.VisualType == PieceVisualType.Standard && IsGoalActive(GoalType.CollectColor, piece.Color))
        {
            return 1;
        }

        return 0;
    }
    
    private bool IsGoalActive(GoalType type, PieceColor? color)
    {
        for (int i = 0; i < _goals.Count; i++)
        {
            if (_goals[i].Type != type)
            {
                continue;
            }
            if (type == GoalType.CollectColor && _goals[i].Color != color)
            {
                continue;
            }
            if (_remaining[i] > 0)
            {
                return true;
            }
        }
        return false;
    }
}
