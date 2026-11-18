using System;
using LogicLibrary;

public sealed class HoleElement : ILevelElement
{
    public char Code => '#';
    public LevelLayer Layer => LevelLayer.Cell;
    public string DisplayName => "Hole";
    public GoalType? AutoGoal => null;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = null;
        if (level.At(LevelLayer.Overlay, x, y) != LevelData.Empty || level.TryGetCage(x, y, out _, out _))
        {
            reason = "A hole can't be under a lock or cage.";
        }
        else if (level.At(LevelLayer.Floor, x, y) != LevelData.Empty)
        {
            reason = "A hole can't have a floor.";
        }
        return reason == null;
    }

    public void Place(Board board, int x, int y) => board.SetCell(x, y, new HolePiece(board.NextId()));
}

public sealed class BoxElement : ILevelElement
{
    public char Code => 'B';
    public LevelLayer Layer => LevelLayer.Cell;
    public string DisplayName => "Box";
    public GoalType? AutoGoal => GoalType.ClearBox;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = level.At(LevelLayer.Overlay, x, y) == LockElement.LockCode ? "A box can't be locked." : null;
        return reason == null;
    }

    public void Place(Board board, int x, int y)
    {
        board.SetCell(x, y, new BoxPiece(board.NextId(), PieceColor.Empty));
    }
}

public sealed class ToyElement : ILevelElement
{
    public char Code => 'O';
    public LevelLayer Layer => LevelLayer.Cell;
    public string DisplayName => "Toy";
    public GoalType? AutoGoal => GoalType.CollectToy;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = null; return true;
    }
    public void Place(Board board, int x, int y)
    {
        board.SetCell(x, y, new ToyPiece(board.NextId()));
    }
}

public sealed class ColorElement : ILevelElement
{
    private readonly PieceColor _color;

    public ColorElement(PieceColor color, char code)
    {
        _color = color;
        Code = code;
    }

    public char Code { get; }
    public LevelLayer Layer => LevelLayer.Cell;
    public string DisplayName => _color.ToString();
    public GoalType? AutoGoal => null;
    public PieceColor Color => _color;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = Array.IndexOf(level.colors, _color.ToString()) < 0 ? $"{_color} is not one of the level's colors." : null;
        return reason == null;
    }

    public void Place(Board board, int x, int y)
    {
        board.SetCell(x, y, new ColoredPiece(board.NextId(), _color));
    }
}

public sealed class SpecialElement : ILevelElement
{
    private readonly Func<int, Piece> _create;

    public SpecialElement(char code, string displayName, Func<int, Piece> create)
    {
        Code = code;
        DisplayName = displayName;
        _create = create;
    }

    public char Code { get; }
    public LevelLayer Layer => LevelLayer.Cell;
    public string DisplayName { get; }
    public GoalType? AutoGoal => null;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = null; return true;
    }
    public void Place(Board board, int x, int y)
    {
        board.SetCell(x, y, _create(board.NextId()));
    }
}