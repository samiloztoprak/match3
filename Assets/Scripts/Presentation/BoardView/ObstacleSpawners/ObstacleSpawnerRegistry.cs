using System.Collections.Generic;

public static class ObstacleSpawnerRegistry
{
    private static readonly Dictionary<ObstacleType, IObstacleSpawner> _spawners = new Dictionary<ObstacleType, IObstacleSpawner>
    {
        { ObstacleType.Box, new BoxObstacleSpawner() },
        { ObstacleType.Lock, new LockObstacleSpawner() },
        { ObstacleType.Grass, new GrassObstacleSpawner() },
        { ObstacleType.Cage, new CageObstacleSpawner() },
        { ObstacleType.Hole, new HoleObstacleSpawner() },
        { ObstacleType.Toy, new ToyObstacleSpawner() }
    };

    public static IObstacleSpawner Get(ObstacleType type)
    {
        _spawners.TryGetValue(type, out var spawner);
        return spawner;
    }
}