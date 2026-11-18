
public interface ILevelSource
{
    int Count { get; }
    LevelData Get(int index);
}