
using System.Collections.Generic;
using System.Linq;
using LogicLibrary;

public static class TurnResolver
{
    private readonly struct PendingActivation
    {
        public readonly Piece Piece;
        public readonly int X;
        public readonly int Y;
        public readonly Piece Partner;

        public PendingActivation(Piece piece, int x, int y, Piece partner)
        {
            Piece = piece;
            X = x;
            Y = y;
            Partner = partner;
        }
    }
    
    private static readonly int MaxIteration = 100;
    public static List<TurnStepData> Resolve(Board board, ICommand command)
    {
        var result = new List<TurnStepData>();
        
        if(command != null)
            command.Execute();

        bool specialActivated = false;
        var pending = new Queue<PendingActivation>();

        if (command is SwapCommand swapCmd)
        {
            Piece atSource = board.GetCell(swapCmd.SourcePosition.x, swapCmd.SourcePosition.y);
            Piece atTarget = board.GetCell(swapCmd.TargetPosition.x, swapCmd.TargetPosition.y);

            bool sourceIsSpecial = atSource != null && atSource.VisualType == PieceVisualType.Special;
            bool targetIsSpecial = atTarget != null && atTarget.VisualType == PieceVisualType.Special;

            if (sourceIsSpecial && targetIsSpecial)
            {
                PieceKind kindA = GetKind(atSource);
                PieceKind kindB = GetKind(atTarget);

                int comboX = swapCmd.TargetPosition.x;
                int comboY = swapCmd.TargetPosition.y;

                var consumed = new HashSet<int> { atSource.Id, atTarget.Id };
                var ownCells = new HashSet<(int x, int y)> { swapCmd.SourcePosition, (comboX, comboY) };

                bool exactlyOneIsLightBall =
                    (kindA == PieceKind.LightBall) != (kindB == PieceKind.LightBall);

                if (exactlyOneIsLightBall)
                {
                    PieceKind otherKind =
                        kindA == PieceKind.LightBall ? kindB : kindA;

                    var transformed = SpecialActivationLogic.TransformMostCommonColorToKind(board, otherKind);
                    var transformClear = new ClearPieceData();

                    foreach (var (oldPiece, newPiece, tx, ty) in transformed)
                    {
                        transformClear.PieceIds.Add(oldPiece.Id);
                        pending.Enqueue(new PendingActivation(newPiece, tx, ty, null));
                    }

                    result.Add(transformClear);
                }
                else
                {
                    RocketDirection? rocketDirection = null;
                    if (atSource is RocketPiece rocketA)
                    {
                        rocketDirection = rocketA.Direction;
                    }
                    else if (atTarget is RocketPiece rocketB)
                    {
                        rocketDirection = rocketB.Direction;
                    }
                    
                    var affected = SpecialActivationLogic.GetComboAffectedCells(board, kindA, kindB, comboX, comboY, rocketDirection);
                    ownCells.UnionWith(affected);
                }

                result.Add(new SpecialActivatedData { PieceId = atTarget.Id, Kind = kindB, X = comboX, Y = comboY });
                result.Add(ClearCellsAndEnqueueChains(board, ownCells, pending, consumed));
                specialActivated = true;
            }
            else if (sourceIsSpecial)
            {
                pending.Enqueue(new PendingActivation(atSource, swapCmd.SourcePosition.x, swapCmd.SourcePosition.y,
                    atTarget));
            }
            else if (targetIsSpecial)
            {
                pending.Enqueue(new PendingActivation(atTarget, swapCmd.TargetPosition.x, swapCmd.TargetPosition.y,
                    atSource));
            }
        }
        else if (command is TapCommand tapCommand)
        {
            Piece atTap = board.GetCell(tapCommand.SourcePosition.x, tapCommand.SourcePosition.y);
            bool tapIsSpecial = atTap != null
                                && atTap.VisualType == PieceVisualType.Special
                                && !BoardLogic.IsPieceLocked(atTap);

            if (tapIsSpecial)
            {
                pending.Enqueue(new PendingActivation(atTap, tapCommand.SourcePosition.x, tapCommand.SourcePosition.y, null));
            }
        }

        while (pending.Count > 0)
        {
            PendingActivation activation = pending.Dequeue();

            if (board.GetCell(activation.X, activation.Y) != activation.Piece)
            {
                continue;
            }

            var affected = SpecialActivationLogic.GetAffectedCells(board, activation.Piece, activation.X,
                activation.Y, activation.Partner);
            var consumed = new HashSet<int> { activation.Piece.Id };

            result.Add(new SpecialActivatedData
            {
                PieceId = activation.Piece.Id, Kind = GetKind(activation.Piece), X = activation.X, Y = activation.Y
            });
            result.Add(ClearCellsAndEnqueueChains(board, affected, pending, consumed));
            specialActivated = true;
        }



        int iteration = 0;
        while (true)
        {
            List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = MatchLogic.FindMatches(board);
            if (matches.Count == 0)
            {
                bool skipBreakForActivation = specialActivated && iteration == 0;
                if (!skipBreakForActivation)
                {
                    if (iteration == 0)
                    {
                        //Restore execution.
                        if (command != null)
                            command.Execute();
                    }
                    break;
                }
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
    
    private static PieceKind GetKind(Piece piece)
    {
        if (piece is RocketPiece) return PieceKind.Rocket;
        if (piece is SpecialPiece sp) return sp.Kind;
        return PieceKind.Normal;
    }

    private static ClearPieceData ClearCellsAndEnqueueChains(Board board, IEnumerable<(int x, int y)> cells, Queue<PendingActivation> pending, HashSet<int> excludeFromChain)
    {
        var clearPieceData = new ClearPieceData();
        foreach (var (cx, cy) in cells)
        {
            Piece piece = board.GetCell(cx, cy);
            if (piece == null) continue;

            bool isChainTarget = piece.VisualType == PieceVisualType.Special && !excludeFromChain.Contains(piece.Id);
            if (isChainTarget)
            {
                pending.Enqueue(new PendingActivation(piece, cx, cy, null));
                continue;
            }

            clearPieceData.PieceIds.Add(piece.Id);
            board.ClearCell(cx, cy);
        }
        return clearPieceData;
    }
}
