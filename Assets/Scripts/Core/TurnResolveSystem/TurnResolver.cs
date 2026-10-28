
using System.Collections.Generic;
using System.Linq;
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
            List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = MatchLogic.FindMatches(board);
            if (matches.Count == 0)
            {
                if(iteration == 0)
                {
                    //Restore execution.
                    if (command != null)
                        command.Execute();
                }
                break;
            }

            HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
            foreach (var (template, cells) in matches)
            foreach (var cell in cells)
                matchSet.Add(cell);
            
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
            
            (int x, int y)? anchorHint = null;
            if (iteration == 0 && command is SwapCommand swapCommand)
            {
                anchorHint = swapCommand.TargetPosition;
            }
            
            foreach (var (template, cells) in matches)
            {
                bool producesSpecial = template.Produces != PieceKind.Normal;
                if (!producesSpecial)
                {
                    foreach (var cell in cells)
                    {
                        Piece piece = board.GetCell(cell.Item1, cell.Item2);
                        clearStepData.PieceIds.Add(piece.Id);
                        board.ClearCell(cell.Item1, cell.Item2);
                    }
                }
                else
                {
                    (int, int) anchor = (anchorHint.HasValue && cells.Contains(anchorHint.Value))
                        ? anchorHint.Value
                        : cells.First();

                    List<int> mergedIds = new List<int>();
                    
                    foreach (var cell in cells)
                    {
                        Piece piece = board.GetCell(cell.Item1, cell.Item2);
                        mergedIds.Add(piece.Id);
                        
                        if(cell == anchor)
                            continue;
                        
                        clearStepData.PieceIds.Add(piece.Id);
                        board.ClearCell(cell.Item1, cell.Item2);
                    }
                    
                    Piece anchorPiece = board.GetCell(anchor.Item1, anchor.Item2);
                    clearStepData.PieceIds.Add(anchorPiece.Id);
                    board.ClearCell(anchor.Item1, anchor.Item2);
                    
                    Piece newSpecial;
                    if (template.Produces == PieceKind.Rocket)
                    {
                        newSpecial = new RocketPiece(board.NextId(), MatchLogic.ResolveRocketDirection(cells));
                    }
                    else
                    {
                        newSpecial = new SpecialPiece(board.NextId(), template.Produces);
                    }
                    board.SetCell(anchor.Item1, anchor.Item2, newSpecial);

                    result.Add(new SpecialCreatedData
                    {
                        Piece = newSpecial,
                        PieceId = newSpecial.Id,
                        Kind = template.Produces,
                        X = anchor.Item1,
                        Y = anchor.Item2,
                        MergedIds = mergedIds
                    });
                }
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
