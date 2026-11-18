
public interface ILevelElement
{
    char Code { get; }
    LevelLayer Layer { get; }
    string DisplayName { get; }
    GoalType? AutoGoal { get; }

    bool CanPlace(LevelData level, int x, int y, out string reason);
    void Place(Board board, int x, int y);
}