using UnityEngine;
using LogicLibrary;

public sealed class LockObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Lock;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.AfterFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        Vector2Int cell = obstacle.Cells[0];
        LockPiece lockPiece = new LockPiece(board.NextId(), cell.x, cell.y);
        board.AddOverlayPiece(lockPiece);

        Piece covered = board.GetCell(cell.x, cell.y);
        if (covered != null)
        {
            BoardLogic.AddFlag(covered, PieceFlags.Locked);
        }

        PieceView lockView = turnPlayer.SpawnPieceView(lockPiece, turnPlayer.CellToWorld(cell.x, cell.y));
        lockView.SetSortingOrder(SortingOrders.Overlay);
    }
}