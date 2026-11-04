
public interface IObstacleSpawner
{
    ObstacleType Type { get; }
    ObstacleSpawnPhase Phase { get; }
    void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle);
}