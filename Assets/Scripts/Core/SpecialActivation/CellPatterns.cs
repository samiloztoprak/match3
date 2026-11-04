using System.Collections.Generic;

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
    
    public static (int x, int y) RandomCell(Board board, IRandom random)
    {
        return (random.Next(board.Width), random.Next(board.Height));
    }
    
    public static (int x, int y) PickTarget(Board board, IRandom random, ITargetPriority targetPriority)
    {
        if (targetPriority != null)
        {
            List<(int x, int y)> bestCells = new List<(int x, int y)>();
            int bestPriority = 0;

            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
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
        return RandomCell(board, random);
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
