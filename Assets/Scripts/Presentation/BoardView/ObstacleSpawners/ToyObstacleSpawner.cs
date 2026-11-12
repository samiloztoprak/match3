using UnityEngine;

public sealed class ToyObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Toy;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.BeforeFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        foreach (Vector2Int cell in obstacle.Cells)
        {
            board.SetCell(cell.x, cell.y, new ToyPiece(board.NextId()));
        }
    }
}