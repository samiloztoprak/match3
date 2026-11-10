using UnityEngine;

public sealed class HoleObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Hole;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.BeforeFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        Vector2Int cell = obstacle.Cells[0];
        board.SetCell(cell.x, cell.y, new HolePiece(board.NextId()));
    }
}