using UnityEngine;

public sealed class GrassObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Grass;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.AfterFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        Vector2Int cell = obstacle.Cells[0];
        GrassPiece grassPiece = new GrassPiece(board.NextId());
        board.SetFloor(cell.x, cell.y, grassPiece);

        PieceView grassView = turnPlayer.SpawnPieceView(grassPiece, turnPlayer.CellToWorld(cell.x, cell.y));
        grassView.SetSortingOrder(SortingOrders.Floor);
    }
}