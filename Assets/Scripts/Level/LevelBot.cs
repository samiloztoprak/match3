using System;
using System.Collections.Generic;
using System.Linq;
using LogicLibrary;

public enum LevelDifficulty
{
    Easy,
    Normal,
    Hard,
    VeryHard,
    TooHard,
}

public static class LevelBot
{
    public const float TargetWinRate = 0.8f;
    private const int MaxMoves = 300;

    private const float GoalWeight = 10f;
    private const float SpecialWeight = 3f;
    private const float ToyWeight = 1f;
    private const float ClearWeight = 0.1f;

    public sealed class RunResult
    {
        public int MovesNeeded = -1;
        public string LastGoal; 
        public bool Finished => MovesNeeded >= 0;
    }

    public sealed class Report
    {
        public int Runs;
        public int MoveLimit;
        public float WinRate;
        public int MedianMoves = -1;
        public int SuggestedMoves = -1;
        public int SuggestedTwoStars, SuggestedThreeStars;
        public string HardestGoal;
        public float HardestGoalShare;
        public int Unfinished;
        public LevelDifficulty Difficulty;
    }

    private readonly struct Move
    {
        public readonly int X1, Y1, X2, Y2;
        public readonly bool IsTap;

        public Move(int x1, int y1, int x2, int y2, bool isTap)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; IsTap = isTap;
        }

        public ICommand ToCommand(Board board) =>
            IsTap ? new TapCommand(X1, Y1) : (ICommand)new SwapCommand(board, X1, Y1, X2, Y2);
    }
    
    public static RunResult Play(LevelData level, int seed)
    {
        var random = new SystemRandom(seed);
        var lookahead = new SystemRandom(seed ^ 0x5bd1e995);
        Board board = LevelBoardBuilder.Build(level, random);
        var goals = new GoalTracker(board, LevelBoardBuilder.BuildGoals(level));
        var result = new RunResult();

        ApplyProgress(TurnResolver.Resolve(board, null, random, goals), goals);
        if (goals.IsComplete())
        {
            result.MovesNeeded = 0;
            return result;
        }

        int cap = Math.Min(MaxMoves, Math.Max(level.moves * 3, level.moves + 30));
        for (int move = 1; move <= cap; move++)
        {
            if (!MoveLogic.HasAvailableMove(board))
            {
                TurnResolver.ResolveShuffle(board, random);
                if (!MoveLogic.HasAvailableMove(board))
                {
                    break;
                }
            }

            Move? best = FindBestMove(board, goals, lookahead);
            if (!best.HasValue)
            {
                break;
            }

            List<string> openBefore = OpenGoals(goals);
            ApplyProgress(TurnResolver.Resolve(board, best.Value.ToCommand(board), random, goals), goals);

            if (goals.IsComplete())
            {
                result.MovesNeeded = move;
                result.LastGoal = openBefore.FirstOrDefault();
                return result;
            }
        }

        result.LastGoal = FurthestGoal(goals);
        return result;
    }

    private static Move? FindBestMove(Board board, GoalTracker goals, IRandom lookahead)
    {
        Move? best = null;
        float bestScore = float.MinValue;
        int toysBefore = ToyDepth(board);

        foreach (Move move in Candidates(board))
        {
            Board copy = board.Clone();
            List<TurnStepData> steps = TurnResolver.Resolve(copy, move.ToCommand(copy), lookahead, goals);
            if (steps.Count == 0)
            {
                continue;
            }

            float score = Score(steps, goals, copy, toysBefore) + lookahead.Next(1000) * 0.00001f;   // random tie-break
            if (score > bestScore)
            {
                bestScore = score;
                best = move;
            }
        }
        return best;
    }

    private static IEnumerable<Move> Candidates(Board board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                Piece piece = board.GetCell(x, y);
                
                if (piece != null && piece.VisualType == PieceVisualType.Special && !BoardLogic.IsPieceLocked(piece))
                {
                    yield return new Move(x, y, x, y, true);
                }
                if (x + 1 < board.Width && MoveLogic.IsValidSwap(board, x, y, x + 1, y))
                {
                    yield return new Move(x, y, x + 1, y, false);
                }
                if (y + 1 < board.Height && MoveLogic.IsValidSwap(board, x, y, x, y + 1))
                {
                    yield return new Move(x, y, x, y + 1, false);
                }
            }
        }
    }

    private static float Score(List<TurnStepData> steps, GoalTracker goals, Board after, int toysBefore)
    {
        float score = 0f;
        int collectedToys = 0;

        foreach (TurnStepData step in steps)
        {
            if (step is SpecialCreatedData)
            {
                score += SpecialWeight;
            }
            else if (step is GoalProgressData progress)
            {
                score += goals.CountUseful(progress) * GoalWeight;
                score += progress.ClearedColors.Count * ClearWeight;
                collectedToys = progress.GetCount(GoalType.CollectToy);
            }
        }

        int toysAfter = ToyDepth(after) + collectedToys * after.Height;
        return score + (toysAfter - toysBefore) * ToyWeight;
    }

    private static int ToyDepth(Board board)
    {
        int depth = 0;
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                Piece piece = board.GetCell(x, y);
                if (piece != null && piece.Flags.HasFlag(PieceFlags.Collectable)) depth += y;
            }
        }
        return depth;
    }

    private static void ApplyProgress(List<TurnStepData> steps, GoalTracker goals)
    {
        foreach (TurnStepData step in steps)
        {
            if (step is GoalProgressData progress)
            {
                goals.ApplyProgress(progress);
                return;
            }
        }
    }

    private static List<string> OpenGoals(GoalTracker goals)
    {
        var open = new List<string>();
        for (int i = 0; i < goals.Goals.Count; i++)
        {
            if (goals.GetRemaining(i) > 0) open.Add(Label(goals.Goals[i]));
        }
        return open;
    }

    private static string FurthestGoal(GoalTracker goals)
    {
        string furthest = null;
        float worst = -1f;
        for (int i = 0; i < goals.Goals.Count; i++)
        {
            float left = goals.GetRemaining(i) / (float)Math.Max(1, goals.Goals[i].TargetCount);
            if (left > worst)
            {
                worst = left;
                furthest = Label(goals.Goals[i]);
            }
        }
        return furthest;
    }

    private static string Label(LevelGoal goal) =>
        goal.Type == GoalType.CollectColor ? $"Collect {goal.Color}" : goal.Type.ToString();

    public static Report Analyze(IReadOnlyList<RunResult> runs, int moveLimit)
    {
        var report = new Report { Runs = runs.Count, MoveLimit = moveLimit };
        if (runs.Count == 0) return report;

        List<int> needed = Needed(runs);
        List<int> finished = needed.Where(n => n != int.MaxValue).ToList();

        report.Unfinished = runs.Count - finished.Count;
        report.WinRate = needed.Count(n => n <= moveLimit) / (float)runs.Count;
        report.Difficulty = Rate(report.WinRate);
        if (finished.Count > 0) report.MedianMoves = Percentile(finished, 0.5f);

        int target = Percentile(needed, TargetWinRate);
        report.SuggestedMoves = target == int.MaxValue ? -1 : target;

        int three = moveLimit - Percentile(needed, 0.2f);
        int two = moveLimit - Percentile(needed, 0.5f);
        if (three >= 1)
        {
            report.SuggestedThreeStars = Math.Min(three, moveLimit - 1);
            report.SuggestedTwoStars = Math.Max(1, Math.Min(two, report.SuggestedThreeStars));
        }

        var hardest = runs.Where(r => r.LastGoal != null).GroupBy(r => r.LastGoal).OrderByDescending(g => g.Count()).FirstOrDefault();
        if (hardest != null)
        {
            report.HardestGoal = hardest.Key;
            report.HardestGoalShare = hardest.Count() / (float)runs.Count;
        }
        return report;
    }

    private static List<int> Needed(IReadOnlyList<RunResult> runs)
    {
        return runs.Select(r => r.Finished ? r.MovesNeeded : int.MaxValue).OrderBy(n => n).ToList();
    }
    
    private static int Percentile(List<int> sorted, float p)
    {
        int index = (int)Math.Ceiling(p * sorted.Count - 0.0001f) - 1;
        return sorted[Math.Max(0, Math.Min(sorted.Count - 1, index))];
    }
    
    public static LevelDifficulty Rate(float winRate)
    {
        if (winRate >= 0.9f) return LevelDifficulty.Easy;
        if (winRate >= 0.7f) return LevelDifficulty.Normal;
        if (winRate >= 0.45f) return LevelDifficulty.Hard;
        if (winRate >= 0.25f) return LevelDifficulty.VeryHard;
        return LevelDifficulty.TooHard;
    }
    
    public static float WinRateFor(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:     return 0.95f;
            case LevelDifficulty.Normal:   return 0.8f;
            case LevelDifficulty.Hard:     return 0.57f;
            case LevelDifficulty.VeryHard: return 0.35f;
            default:                       return 0.15f;
        }
    }
    
    public static int MovesFor(IReadOnlyList<RunResult> runs, LevelDifficulty difficulty)
    {
        if (runs.Count == 0)
        {
            return -1;
        }
        int moves = Percentile(Needed(runs), WinRateFor(difficulty));
        return moves == int.MaxValue ? -1 : moves;
    }
}