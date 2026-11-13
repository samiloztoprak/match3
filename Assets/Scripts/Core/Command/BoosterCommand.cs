using LogicLibrary;

public class BoosterCommand : ICommand
{
    public BoosterType Type { get; }
    public int X { get; }
    public int Y { get; }

    public BoosterCommand(BoosterType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    public bool Validate(Board board)
    {
        if (BoosterRegistry.Get(Type) == null) return false;
        if (!BoardLogic.IsValidIndex(X, Y, board)) return false;
        return !BoardLogic.IsVoid(board.GetCell(X, Y));
    }

    public void Execute()
    {
    }
}