using LogicLibrary;

public sealed class LockElement : ILevelElement
{
    public const char LockCode = 'L';

    public char Code => LockCode;
    public LevelLayer Layer => LevelLayer.Overlay;
    public string DisplayName => "Lock";
    public GoalType? AutoGoal => GoalType.ClearLock;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = null;
        char cell = level.At(LevelLayer.Cell, x, y);
        if (cell == '#')
        {
            reason = "A lock can't be on a hole.";
        }
        else if (cell == 'B')
        {
            reason = "A lock can't be on a box.";
        }
        else if (level.TryGetCage(x, y, out _, out _))
        {
            reason = "A lock can't be inside a cage.";
        }
        return reason == null;
    }

    public void Place(Board board, int x, int y)
    {
        board.AddOverlayPiece(new LockPiece(board.NextId(), x, y));
        Piece covered = board.GetCell(x, y);
        if (covered != null)
        {
            BoardLogic.AddFlag(covered, PieceFlags.Locked);
        }
    }
}

public sealed class CageElement : ILevelElement
{
    public const char CageCode = 'C';

    public char Code => CageCode;
    public LevelLayer Layer => LevelLayer.Overlay;
    public string DisplayName => "Cage (2x2)";
    public GoalType? AutoGoal => null;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = null;
        if (!level.InBounds(x + 1, y + 1))
        {
            reason = "The cage doesn't fit on the board."; return false;
        }

        for (int dy = 0; dy <= 1; dy++)
        {
            for (int dx = 0; dx <= 1; dx++)
            {
                int cx = x + dx, cy = y + dy;
                if (level.At(LevelLayer.Cell, cx, cy) == '#')
                {
                    reason = "The cage covers a hole.";
                    return false;
                }

                bool isSelf = dx == 0 && dy == 0;
                if (!isSelf && level.At(LevelLayer.Overlay, cx, cy) != LevelData.Empty)
                {
                    reason = "The cage overlaps another overlay.";
                    return false;
                }
            }
        }

        for (int ay = y - 1; ay <= y + 1; ay++)
        {
            for (int ax = x - 1; ax <= x + 1; ax++)
            {
                if ((ax != x || ay != y) && level.InBounds(ax, ay) && level.At(LevelLayer.Overlay, ax, ay) == CageCode)
                {
                    reason = "The cage overlaps another cage.";
                    return false;
                }
            }
        }
        return true;
    }

    public void Place(Board board, int x, int y)
    {
        var cage = new CagePiece(board.NextId(), x, y);
        board.AddOverlayPiece(cage);
        foreach (var (cx, cy) in cage.CoveredCells)
        {
            Piece covered = board.GetCell(cx, cy);
            if (covered != null)
            {
                BoardLogic.AddFlag(covered, PieceFlags.Locked);
            }
        }
    }
}