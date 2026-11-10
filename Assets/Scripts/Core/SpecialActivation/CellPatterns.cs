using System.Collections.Generic;
using LogicLibrary;

public static class CellPatterns
{
    public static void AddRowAndColumn(Board board, int x, int y, HashSet<(int x, int y)> cells)
    {
        for (int cx = 0; cx < board.Width; cx++)
        {
            cells.Add((cx, y));
        }
        for (int cy = 0; cy < board.Height; cy++)
        {
            cells.Add((x, cy));
        }
    }

    public static HashSet<(int x, int y)> ToCellSet(List<(int x, int y, float timeMs)> cells)
    {
        var set = new HashSet<(int x, int y)>();
        foreach (var (cx, cy, _) in cells)
        {
            set.Add((cx, cy));
        }
        return set;
    }
    
    public static List<(int x, int y, float timeMs)> PropellerTakeoff(Board board, int x, int y)
    {
        var cells = new List<(int x, int y, float timeMs)> { (x, y, 0f) };
        AddNeighbors(board, x, y, cells);
        return cells;
    }
    
    public static void AddArea(Board board, int x, int y, int radius, HashSet<(int x, int y)> cells)
    {
        for (int cx = x - radius; cx <= x + radius; cx++)
        {
            for (int cy = y - radius; cy <= y + radius; cy++)
            {
                if (cx < 0 || cx >= board.Width || cy < 0 || cy >= board.Height)
                {
                    continue;
                }
                cells.Add((cx, cy));
            }
        }
    }

    public static void AddThickCross(Board board, int x, int y, int thickness, HashSet<(int x, int y)> cells)
    {
        for (int oy = -thickness; oy <= thickness; oy++)
        {
            int ry = y + oy;
            if (ry < 0 || ry >= board.Height)
            {
                continue;
            }
            for (int cx = 0; cx < board.Width; cx++)
            {
                cells.Add((cx, ry));
            }
        }
        for (int ox = -thickness; ox <= thickness; ox++)
        {
            int rx = x + ox;
            if (rx < 0 || rx >= board.Width)
            {
                continue;
            }
            for (int cy = 0; cy < board.Height; cy++)
            {
                cells.Add((rx, cy));
            }
        }
    }
    
    public static List<(int x, int y)> AddNeighbors(Board board, int x, int y, List<(int x, int y, float timeMs)> cells)
    {
        var neighbors = new List<(int x, int y)>();
        (int dx, int dy)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx;
            int ny = y + dy;
            if (nx < 0 || nx >= board.Width || ny < 0 || ny >= board.Height)
            {
                continue;
            }
            cells.Add((nx, ny, 0f));
            neighbors.Add((nx, ny));
        }
        return neighbors;
    }

    public static void AddDirectional(Board board, int x, int y, RocketDirection direction, HashSet<(int x, int y)> cells)
    {
        if (direction == RocketDirection.Horizontal)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                cells.Add((cx, y));
            }
        }
        else
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                cells.Add((x, cy));
            }
        }
    }
    
    public static List<(int x, int y, float timeMs)> AddRowAndColumnTimed(Board board, int x, int y)
    {
        var map = new Dictionary<(int x, int y), float>();
        for (int cx = 0; cx < board.Width; cx++)
        {
            map[(cx, y)] = ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - x);
        }
        for (int cy = 0; cy < board.Height; cy++)
        {
            float t = ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - y);
            if (!map.TryGetValue((x, cy), out float existing) || t < existing)
            {
                map[(x, cy)] = t;
            }
        }
        var result = new List<(int x, int y, float timeMs)>();
        foreach (var kvp in map) result.Add((kvp.Key.x, kvp.Key.y, kvp.Value));
        return result;
    }

    public static List<(int x, int y, float timeMs)> AddThickCrossTimed(Board board, int x, int y, int thickness)
    {
        var map = new Dictionary<(int x, int y), float>();
        for (int oy = -thickness; oy <= thickness; oy++)
        {
            int ry = y + oy;
            if (ry < 0 || ry >= board.Height) continue;
            for (int cx = 0; cx < board.Width; cx++)
            {
                float t = ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - x);
                if (!map.TryGetValue((cx, ry), out float existing) || t < existing) map[(cx, ry)] = t;
            }
        }
        for (int ox = -thickness; ox <= thickness; ox++)
        {
            int rx = x + ox;
            if (rx < 0 || rx >= board.Width) continue;
            for (int cy = 0; cy < board.Height; cy++)
            {
                float t = ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - y);
                if (!map.TryGetValue((rx, cy), out float existing) || t < existing) map[(rx, cy)] = t;
            }
        }
        var result = new List<(int x, int y, float timeMs)>();
        foreach (var kvp in map) result.Add((kvp.Key.x, kvp.Key.y, kvp.Value));
        return result;
    }
    
    public static (int x, int y) RandomCell(Board board, IRandom random, HashSet<(int x, int y)> exclude = null)
    {
        var candidates = new List<(int x, int y)>();
        for (int cx = 0; cx < board.Width; cx++)
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                Piece cell = board.GetCell(cx, cy);

                if (cell == null)
                {
                    continue;
                }
                
                if(BoardLogic.IsVoid(cell))
                {
                    continue;
                }
                
                if (exclude != null && exclude.Contains((cx, cy)))
                {
                    continue;
                }
                candidates.Add((cx, cy));
            }
        }

        if (candidates.Count == 0)
        {
            return (random.Next(board.Width), random.Next(board.Height));
        }

        return candidates[random.Next(candidates.Count)];
    }
    
    public static (int x, int y) PickTarget(Board board, IRandom random, ITargetPriority targetPriority, HashSet<(int x, int y)> exclude = null)
    {
        if (targetPriority != null)
        {
            List<(int x, int y)> bestCells = new List<(int x, int y)>();
            int bestPriority = 0;

            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    if (exclude != null && exclude.Contains((cx, cy)))
                    {
                        continue;
                    }
                    int priority = targetPriority.GetPriority(board, cx, cy);
                    if (priority <= 0)
                    {
                        continue;
                    }
                    if (priority > bestPriority)
                    {
                        bestPriority = priority;
                        bestCells.Clear();
                    }
                    if (priority == bestPriority)
                    {
                        bestCells.Add((cx, cy));
                    }
                }
            }

            if (bestCells.Count > 0)
            {
                return bestCells[random.Next(bestCells.Count)];
            }
        }
        return RandomCell(board, random, exclude);
    }
    
    public static PieceColor GetMostCommonColor(Board board)
    {
        var counts = new Dictionary<PieceColor, int>();
        for (int cx = 0; cx < board.Width; cx++)
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                Piece p = board.GetCell(cx, cy);
                if (p == null || p.VisualType != PieceVisualType.Standard)
                {
                    continue;
                }

                counts.TryGetValue(p.Color, out int count);
                counts[p.Color] = count + 1;
            }
        }

        PieceColor best = PieceColor.Empty;
        int bestCount = 0;
        foreach (var kvp in counts)
        {
            if (kvp.Value > bestCount)
            {
                best = kvp.Key;
                bestCount = kvp.Value;
            }
        }
        return best;
    }
}
