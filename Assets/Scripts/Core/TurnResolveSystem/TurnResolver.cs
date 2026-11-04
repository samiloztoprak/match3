
using System.Collections.Generic;
using System.Linq;
using LogicLibrary;

public static class TurnResolver
{
    private static readonly IRandom DefaultRandom = new SystemRandom();
    
    private readonly struct PendingActivation
    {
        public readonly Piece Piece;
        public readonly int X;
        public readonly int Y;
        public readonly Piece Partner;
        public readonly float TimeMs;

        public PendingActivation(Piece piece, int x, int y, Piece partner, float timeMs)
        {
            Piece = piece;
            X = x;
            Y = y;
            Partner = partner;
            TimeMs = timeMs;
        }
    }
    
    public static List<TurnStepData> ResolveShuffle(Board board, IRandom random = null)
    {
        if (random == null)
        {
            random = DefaultRandom;
        }

        var moves = ShuffleLogic.Shuffle(board, random);

        var shuffleStepData = new ShuffleStepData();
        shuffleStepData.Moves.AddRange(moves);

        return new List<TurnStepData> { shuffleStepData };
    }
    
    private static readonly int MaxIteration = 100;
    public static List<TurnStepData> Resolve(Board board, ICommand command, IRandom random = null, ITargetPriority targetPriority = null)
    {
        if (random == null)
        {
            random = DefaultRandom;
        }
        
        var result = new List<TurnStepData>();
        var goalProgress = new GoalProgressData();
        
        if (command is SwapCommand swapCmd0 && !swapCmd0.Validate(board))
        {
            return result;
        }
        
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

                RocketDirection? rocketDirection = null;
                if (atSource is RocketPiece rocketA)
                {
                    rocketDirection = rocketA.Direction;
                }
                else if (atTarget is RocketPiece rocketB)
                {
                    rocketDirection = rocketB.Direction;
                }

                IComboActivation combo = ComboActivationRegistry.Get(kindA, kindB);
                ComboEffect comboEffect = combo != null
                    ? combo.GetEffect(board, kindA, kindB, comboX, comboY, rocketDirection, random, targetPriority)
                    : default;

                if (comboEffect.Transformed != null)
                {
                    var transformClear = new ClearPieceData();
                    foreach (var (oldPiece, newPiece, tx, ty) in comboEffect.Transformed)
                    {
                        transformClear.Hits.Add(new HitData(oldPiece.Id, tx, ty, 0f));
                        if (oldPiece.VisualType == PieceVisualType.Standard)
                        {
                            goalProgress.ClearedColors.Add(oldPiece.Color);
                        }
                        pending.Enqueue(new PendingActivation(newPiece, tx, ty, null, ActivationTiming.SwapActivationMs));
                    }
                    result.Add(transformClear);
                }
                else if (comboEffect.Cells != null)
                {
                    ownCells.UnionWith(comboEffect.Cells);
                }
                
                var ownCellsTimed = new List<(int x, int y, float timeMs)>();
                foreach (var (cx, cy) in ownCells)
                {
                    ownCellsTimed.Add((cx, cy, 0f));
                }
                var comboDamageData = new DamagePieceData();
                var comboClearOverlayData = new ClearOverlayData();
                var comboClearData = ClearCellsAndEnqueueChains(board, ownCellsTimed, pending, consumed, comboDamageData, comboClearOverlayData, goalProgress, ActivationTiming.SwapActivationMs);
                result.Add(new SpecialActivatedData { PieceId = atTarget.Id, Kind = kindB, X = comboX, Y = comboY });
                result.Add(comboDamageData);
                result.Add(comboClearOverlayData);
                result.Add(comboClearData);
                specialActivated = true;
            }
            else if (sourceIsSpecial)
            {
                pending.Enqueue(new PendingActivation(atSource, swapCmd.SourcePosition.x, swapCmd.SourcePosition.y,
                    atTarget, ActivationTiming.SwapActivationMs));
            }
            else if (targetIsSpecial)
            {
                pending.Enqueue(new PendingActivation(atTarget, swapCmd.TargetPosition.x, swapCmd.TargetPosition.y,
                    atSource, ActivationTiming.SwapActivationMs));
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
                pending.Enqueue(new PendingActivation(atTap, tapCommand.SourcePosition.x, tapCommand.SourcePosition.y, null, ActivationTiming.SwapActivationMs));
            }
        }

        while (pending.Count > 0)
        {
            PendingActivation activation = pending.Dequeue();

            if (board.GetCell(activation.X, activation.Y) != activation.Piece)
            {
                continue;
            }

            var effect = SpecialActivationLogic.GetAffectedCells(board, activation.Piece, activation.X, activation.Y, activation.Partner, random, targetPriority);
            var consumed = new HashSet<int> { activation.Piece.Id };

            var chainDamageData = new DamagePieceData();
            var chainClearOverlayData = new ClearOverlayData();
            var chainClearData = ClearCellsAndEnqueueChains(board, effect.Cells, pending, consumed, chainDamageData, chainClearOverlayData, goalProgress, activation.TimeMs);
            
            result.Add(new SpecialActivatedData
            {
                PieceId = activation.Piece.Id,
                Kind = GetKind(activation.Piece),
                X = activation.X,
                Y = activation.Y,
                TargetX = effect.FlightTarget?.x,
                TargetY = effect.FlightTarget?.y,
                NeighborCells = effect.NeighborCells
            });
            result.Add(chainDamageData);
            result.Add(chainClearOverlayData);
            result.Add(chainClearData);
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
                    if (iteration == 0 && command is IUndoableCommand undoable)
                    {
                        undoable.Undo();
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
                    clearStepData.Hits.Add(new HitData(piece.Id, x, y, 0f));
                    board.ClearCell(x, y);
                    goalProgress.ClearedBoxCount++;
                }
                else
                {
                    damageData.Hits.Add((piece.Id, piece.Health, 0f));
                }
            }
            
            ClearOverlayData clearOverlayData = new ClearOverlayData();
            List<OverlayPiece> damageableOverlays = MatchLogic.FindNearlyDamageableOverlays(board, matchSet);
            foreach (OverlayPiece overlay in damageableOverlays)
            {
                int overlayX = 0, overlayY = 0;
                foreach (var cell in overlay.CoveredCells)
                {
                    overlayX = cell.x; overlayY = cell.y; break;
                }
                TryBreakOverlay(board, overlay, overlayX, overlayY, 0f, clearOverlayData, damageData, goalProgress);
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
                        clearStepData.Hits.Add(new HitData(piece.Id, cell.Item1, cell.Item2, 0f));
                        if (piece.VisualType == PieceVisualType.Standard)
                        {
                            goalProgress.ClearedColors.Add(piece.Color);
                        }
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
                        
                        if (piece.VisualType == PieceVisualType.Standard)
                        {
                            goalProgress.ClearedColors.Add(piece.Color);
                        }
                        
                        if(cell == anchor)
                            continue;
                        
                        clearStepData.Hits.Add(new HitData(piece.Id, cell.Item1, cell.Item2, 0f));
                        board.ClearCell(cell.Item1, cell.Item2);
                    }
                    
                    Piece anchorPiece = board.GetCell(anchor.Item1, anchor.Item2);
                    clearStepData.Hits.Add(new HitData(anchorPiece.Id, anchor.Item1, anchor.Item2, 0f));
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

            FallStepData fallStepData = new FallStepData();
            RefillStepData refillStepData = new RefillStepData();
            HashSet<int> refilledThisTurn = new HashSet<int>();
            
            int fillIteration = 0;
            const int maxFillIteration = 500;
            while (true)
            {
                List<(int, int, int)> verticalMoves = OrganizationLogic.FillEmpties(board);
                foreach (var move in verticalMoves)
                {
                    RecordFallMove(fallStepData, refillStepData, refilledThisTurn, move);
                }

                var diagonalMove = OrganizationLogic.FillOneDiagonalStep(board, random);
                if (diagonalMove != null)
                {
                    RecordFallMove(fallStepData, refillStepData, refilledThisTurn, diagonalMove.Value);
                    fillIteration++;
                    if (fillIteration > maxFillIteration) break;
                    continue;
                }

                List<(int, PieceColor, int, int)> newPieces = OrganizationLogic.GetNewPieceToEmpty(board, random);
                if (newPieces.Count > 0)
                {
                    foreach (var piece in newPieces)
                    {
                        refillStepData.Moves.Add(piece);
                        refilledThisTurn.Add(piece.Item1);
                    }
                    fillIteration++;
                    if (fillIteration > maxFillIteration)
                    {
                        break;
                    }
                    continue;
                }

                break;
            }
            result.Add(fallStepData);
            result.Add(refillStepData);
            iteration++;
            if (iteration > MaxIteration)
            {
                break;
            }
        }
        
        result.Add(goalProgress);
        return  result;
    }
    
    private static void RecordFallMove(FallStepData fallStepData, RefillStepData refillStepData, HashSet<int> refilledThisTurn, (int pieceId, int toX, int toY) move)
    {
        if (refilledThisTurn.Contains(move.pieceId))
        {
            for (int i = 0; i < refillStepData.Moves.Count; i++)
            {
                if (refillStepData.Moves[i].pieceId == move.pieceId)
                {
                    var entry = refillStepData.Moves[i];
                    refillStepData.Moves[i] = (entry.pieceId, entry.color, move.toX, move.toY);
                    return;
                }
            }
        }

        fallStepData.Moves.Add(move);
    }
    
    private static PieceKind GetKind(Piece piece)
    {
        if (piece is RocketPiece) return PieceKind.Rocket;
        if (piece is SpecialPiece sp) return sp.Kind;
        return PieceKind.Normal;
    }

    private static ClearPieceData ClearCellsAndEnqueueChains(Board board, IEnumerable<(int x, int y, float timeMs)> cells, Queue<PendingActivation> pending, HashSet<int> excludeFromChain, DamagePieceData damageData,
        ClearOverlayData clearOverlayData, GoalProgressData goalProgress, float baseTimeMs)
    {
        var clearPieceData = new ClearPieceData();
        foreach (var (cx, cy, localTimeMs) in cells)
        {
            Piece piece = board.GetCell(cx, cy);
            if (piece == null) continue;

            float absoluteTimeMs = baseTimeMs + localTimeMs;
            
            bool isChainTarget = piece.VisualType == PieceVisualType.Special && !excludeFromChain.Contains(piece.Id);
            if (isChainTarget)
            {
                pending.Enqueue(new PendingActivation(piece, cx, cy, null, absoluteTimeMs + ActivationTiming.ChainDelayMs));
                continue;
            }

            OverlayPiece overlay = board.GetOverlayPieceAt(cx, cy);
            if (overlay != null)
            {
                TryBreakOverlay(board, overlay, cx, cy, absoluteTimeMs, clearOverlayData, damageData, goalProgress);
                if (BoardLogic.IsPieceLocked(piece))
                {
                    continue;
                }
            }
            
            if (piece.Flags.HasFlag(PieceFlags.DamageableByNeighbor))
            {
                bool broken = piece.ApplyDamage(1);
                if (broken)
                {
                    clearPieceData.Hits.Add(new HitData(piece.Id, cx, cy, absoluteTimeMs));
                    board.ClearCell(cx, cy);
                    goalProgress.ClearedBoxCount++;
                }
                else
                {
                    damageData.Hits.Add((piece.Id, piece.Health, absoluteTimeMs));
                }
                continue;
            }
            if (piece.VisualType == PieceVisualType.Standard)
            {
                goalProgress.ClearedColors.Add(piece.Color);
            }
            
            clearPieceData.Hits.Add(new HitData(piece.Id, cx, cy, absoluteTimeMs));
            board.ClearCell(cx, cy);
        }
        return clearPieceData;
    }
    
    private static void TryBreakOverlay(Board board, OverlayPiece overlay, int x, int y, float timeMs,
        ClearOverlayData clearOverlayData, DamagePieceData damageData, GoalProgressData goalProgress)
    {
        bool broken = overlay.ApplyDamage(1);
        if (broken)
        {
            clearOverlayData.Hits.Add(new HitData(overlay.Id, x, y, timeMs));

            if (overlay is LockPiece)
            {
                goalProgress.ClearedLockCount++;
            }
            else if (overlay is GrassPiece)
            {
                goalProgress.ClearedGrassCount++;
            }
            else if (overlay is CagePiece)
            {
                goalProgress.ClearedCageCount++;
            }

            board.RemoveOverlayPiece(overlay);
            foreach (var (ox, oy) in overlay.CoveredCells)
            {
                Piece covered = board.GetCell(ox, oy);
                if (covered != null)
                {
                    BoardLogic.RemoveFlag(covered, PieceFlags.Locked);
                }
            }
        }
        else
        {
            damageData.Hits.Add((overlay.Id, overlay.Health, timeMs));
        }
    }
}
