using UnityEngine;
using LogicLibrary;

public sealed class CageObstacleSpawner : IObstacleSpawner
{
    public ObstacleType Type => ObstacleType.Cage;
    public ObstacleSpawnPhase Phase => ObstacleSpawnPhase.AfterFill;

    public void Spawn(Board board, TurnPlayer turnPlayer, ObstaclePlacement obstacle)
    {
        Vector2Int anchorCell = obstacle.Cells[0];
        CagePiece cagePiece = new CagePiece(board.NextId(), anchorCell.x, anchorCell.y);
        board.AddOverlayPiece(cagePiece);

        foreach (var (x, y) in cagePiece.CoveredCells)
        {
            Piece covered = board.GetCell(x, y);
            if (covered != null)
            {
                BoardLogic.AddFlag(covered, PieceFlags.Locked);
            }
        }

        Vector3 topLeft = turnPlayer.CellToWorld(anchorCell.x, anchorCell.y);
        Vector3 bottomRight = turnPlayer.CellToWorld(anchorCell.x + 1, anchorCell.y + 1);
        Vector3 center = (topLeft + bottomRight) / 2f;

        PieceView cageView = turnPlayer.SpawnPieceView(cagePiece, center);
        cageView.SetSizeInCells(2, 2);
        cageView.SetSortingOrder(1);
    }
}