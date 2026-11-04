using UnityEngine;

public sealed class BoxObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Box;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.BeforeFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        Vector2Int cell = obstacle.Cells[0];
        board.SetCell(cell.x, cell.y, new BoxPiece(board.NextId(), PieceColor.Empty));
    }
}