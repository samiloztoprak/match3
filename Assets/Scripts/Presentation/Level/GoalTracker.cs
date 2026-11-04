using System;
using System.Collections.Generic;

public class GoalTracker : ITargetPriority
{
    private readonly List<LevelGoal> _goals;
    private readonly int[] _remaining;

    public event Action OnProgressChanged;
    public event Action OnAllGoalsComplete;
    
    public GoalTracker(List<LevelGoal> goals)
    {
        _goals = goals;
        _remaining = new int[goals.Count];
        for (int i = 0; i < goals.Count; i++)
        {
            _remaining[i] = goals[i].TargetCount;
        }
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
            switch (_goals[i].Type)
            {
                case GoalType.CollectColor:
                    foreach (var color in progress.ClearedColors)
                    {
                        if (color == _goals[i].Color)
                        {
                            consumed++;
                        }
                    }
                    break;
                case GoalType.ClearBox:
                    consumed = progress.ClearedBoxCount;
                    break;
                case GoalType.ClearLock:
                    consumed = progress.ClearedLockCount;
                    break;
                case GoalType.ClearGrass:
                    consumed = progress.ClearedGrassCount;
                    break;
                case GoalType.ClearCage:
                    consumed = progress.ClearedCageCount;
                    break;
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

        if (piece != null && piece.VisualType == PieceVisualType.Box && IsGoalActive(GoalType.ClearBox, null))
        {
            return 2;
        }

        OverlayPiece overlay = board.GetOverlayPieceAt(x, y);
        if (overlay is LockPiece && IsGoalActive(GoalType.ClearLock, null))
        {
            return 2;
        }
        if (overlay is GrassPiece && IsGoalActive(GoalType.ClearGrass, null))
        {
            return 2;
        }
        if (overlay is CagePiece && IsGoalActive(GoalType.ClearCage, null))
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
