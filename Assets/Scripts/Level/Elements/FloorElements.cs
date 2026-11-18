
public sealed class GrassElement : ILevelElement
{
    public char Code => 'g';
    public LevelLayer Layer => LevelLayer.Floor;
    public string DisplayName => "Grass";
    public GoalType? AutoGoal => GoalType.ClearGrass;

    public bool CanPlace(LevelData level, int x, int y, out string reason)
    {
        reason = level.At(LevelLayer.Cell, x, y) == '#' ? "Grass can't be on a hole." : null;
        return reason == null;
    }

    public void Place(Board board, int x, int y)
    {
        board.SetFloor(x, y, new GrassPiece(board.NextId()));
    }
}