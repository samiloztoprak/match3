
using System.Collections.Generic;
using LogicLibrary;

public static class TurnResolver
{
    private static readonly int MaxIteration = 100;
    public static List<TurnStepData> Resolve(Board board, ICommand command)
    {
        var result = new List<TurnStepData>();
        
        if(command != null)
            command.Execute();
        
        int iteration = 0;
        while (true)
        {
            HashSet<(int, int)> matchSet = MatchLogic.FindMatches(board);
            if (matchSet.Count == 0)
            {
                if(iteration == 0)
                {
                    //Restore execution.
                    if (command != null)
                        command.Execute();
                }
                break;
            }

            ClearPieceData clearStepData = new ClearPieceData();
            DamagePieceData damageData = new DamagePieceData();
            List<(Piece, int, int)> damageablePieces = MatchLogic.FindNearlyDamageable(board, matchSet);
            foreach (var (piece, x, y) in damageablePieces)
            {
                bool broken = piece.ApplyDamage(1);
                if (broken)
                {
                    clearStepData.PieceIds.Add(piece.Id);
                    board.ClearCell(x, y);
                }
                else
                {
                    damageData.PieceIds.Add(piece.Id);
                }
            }
            
            ClearOverlayData clearOverlayData = new ClearOverlayData();
            List<OverlayPiece> damageableOverlays = MatchLogic.FindNearlyDamageableOverlays(board, matchSet);
            foreach (OverlayPiece overlay in damageableOverlays)
            {
                bool broken = overlay.ApplyDamage(1);
                if (broken)
                {
                    clearOverlayData.PieceIds.Add(overlay.Id);
                    board.RemoveOverlayPiece(overlay);
                    foreach (var (x, y) in overlay.CoveredCells)
                    {
                        Piece coveredPiece = board.GetCell(x, y);
                        if (coveredPiece != null)
                        {
                            BoardLogic.RemoveFlag(coveredPiece, PieceFlags.Locked);
                        }
                    }
                }
                else
                {
                    damageData.PieceIds.Add(overlay.Id);
                }
            }
            
            result.Add(damageData);
            result.Add(clearOverlayData);
            
            foreach (var match in matchSet)
            {
                Piece piece = board.GetCell(match.Item1, match.Item2);
                clearStepData.PieceIds.Add(piece.Id);
                board.ClearCell(match.Item1, match.Item2);
            }
            result.Add(clearStepData);

            List<(int, int, int)> fillPieces = OrganizationLogic.FillEmpties(board);
            FallStepData fallStepData = new FallStepData();
            foreach (var piece in fillPieces)
            {
                fallStepData.Moves.Add(piece);
            }
            result.Add(fallStepData);
            
            List<(int, PieceColor, int, int)> newPieces = OrganizationLogic.GetNewPieceToEmpty(board);
            RefillStepData refillStepData = new RefillStepData();
            foreach (var piece in newPieces)
            {
                refillStepData.Moves.Add(piece);
            }
            result.Add(refillStepData);
            
            iteration++;
            if(iteration > MaxIteration)
                break;
        }
        return  result;
    }
}
