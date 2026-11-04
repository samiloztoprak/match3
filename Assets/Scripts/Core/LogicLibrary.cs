using System;
using System.Collections.Generic;

namespace LogicLibrary
{
    public static class BoardLogic
    {
        public static bool IsSwappableIndexes(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1-x2) + Math.Abs(y1 - y2) == 1;
        }

        public static bool IsValidIndex(int x1, int y1, Board board)
        {
            return !(x1 < 0 || y1 < 0 || x1 >= board.Width || y1 >= board.Height);
        }

        public static bool IsPieceLocked(Piece piece)
        {
            return piece.Flags.HasFlag(PieceFlags.Locked);
        }
        
        public static bool IsPieceMoveable(Piece piece)
        {
            return piece.Flags.HasFlag(PieceFlags.Movable);
        }
        
        public static bool IsPieceMatchable(Piece piece)
        {
            return piece.Flags.HasFlag(PieceFlags.CanMatch);
        }
        
        public static bool IsCellSwapBlocked(Board board, int x, int y)
        {
            Piece piece = board.GetCell(x, y);
            if (piece != null && IsPieceLocked(piece))
            {
                return true;
            }
            return board.GetOverlayPieceAt(x, y) is GrassPiece;
        }
        
        public static void AddFlag(Piece piece, PieceFlags flags)
        {
            piece.Flags |= flags;
        }

        public static void RemoveFlag(Piece piece, PieceFlags flags)
        {
            piece.Flags &= ~flags;
        }
    }

    public static class MatchLogic
    {
        public static List<HashSet<(int, int)>> FindClusters(Board board, HashSet<(int, int)> matchSet)
        {
            List<HashSet<(int, int)>> clusters = new List<HashSet<(int, int)>>();
            HashSet<(int,int)> visited = new HashSet<(int, int)>();
            (int directionX, int directionY)[] directions = { (1, 0),  (-1, 0), (0, 1), (0, -1) };

            foreach (var cell in matchSet)
            {
                if (visited.Contains(cell))
                {
                    continue;
                }
                
                HashSet<(int, int)> cluster = new HashSet<(int, int)>();
                Queue<(int, int)> queue = new Queue<(int, int)>();
                queue.Enqueue(cell);
                visited.Add(cell);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    cluster.Add(current);

                    foreach (var (directionX, directionY) in directions)
                    {
                        var neighbor = (current.Item1 + directionX, current.Item2 + directionY);
                        if (matchSet.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                clusters.Add(cluster);
            }
            return  clusters;
        }
        
        public static List<(ShapeTemplate template, HashSet<(int, int)> cells)> FindMatches(Board board)
        {
            List<(ShapeTemplate template, HashSet<(int, int)> cells)> allMatches = new List<(ShapeTemplate, HashSet<(int, int)>)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            (int directionX, int  directionY)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Piece piece = board.GetCell(x, y);
                    if (piece == null || !BoardLogic.IsPieceMatchable(piece) || BoardLogic.IsPieceLocked(piece))
                    {
                        continue;
                    }

                    if (visited.Contains((x, y)))
                    {
                        continue;
                    }
                    
                    HashSet<(int, int)> region = new HashSet<(int, int)>();
                    Queue<(int, int)> queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    visited.Add((x, y));

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        region.Add(current);

                        foreach (var (directionX, directionY) in directions)
                        {
                            var neighbor = (current.Item1 + directionX, current.Item2 + directionY);

                            if (!BoardLogic.IsValidIndex(neighbor.Item1, neighbor.Item2, board) ||
                                visited.Contains(neighbor))
                            {
                                continue;
                            }
                            
                            Piece neighborPiece = board.GetCell(neighbor.Item1, neighbor.Item2);
                            if(neighborPiece == null || !BoardLogic.IsPieceMatchable(neighborPiece)
                                                     || BoardLogic.IsPieceLocked(neighborPiece))
                            {
                                continue;
                            }

                            if (neighborPiece.Color != piece.Color)
                            {
                                continue;
                            }
                            
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                    List<(ShapeTemplate template, HashSet<(int, int)> cells)> matchedShape = FindAllShapeMatchesInRegion(region);
                    allMatches.AddRange(matchedShape);
                }
            }
            return allMatches;
        }

        public static bool TemplateFitsAtOffset(HashSet<(int, int)> templateCells, int offsetX, int offsetY,
            HashSet<(int, int)> cells)
        {
            foreach (var (templateX, templateY) in templateCells)
            {
                if (!cells.Contains((templateX + offsetX, templateY + offsetY)))
                {
                    return false;
                }
            }
            return true;
        }

        public static (ShapeTemplate template, HashSet<(int, int)> matchedCells) FindBestShapeMatchWithPosition(
            HashSet<(int, int)> cells)
        {
            
            // If cells is empty, min/max stay at int sentinel values and overflow happens, freezing the editor (see commit 3352c82)
            if (cells.Count == 0)
            {
                return (null, null);
            }
            
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (var (x, y) in cells)
            {
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            
            foreach (var template in ShapeRegistry.Templates)
            {
                for (int i = 0; i < template.Rotations.Count; i++)
                {
                    var rotation = template.Rotations[i];
                    var (rotMaxX, rotMaxY) = template.RotationBounds[i];
                    
                    for (int offsetX = minX; offsetX <= maxX - rotMaxX; offsetX++)
                    {
                        for (int offsetY =  minY; offsetY <= maxY - rotMaxY; offsetY++)
                        {
                            if (TemplateFitsAtOffset(rotation, offsetX, offsetY, cells))
                            {
                                HashSet<(int, int)> matchedCells = new HashSet<(int, int)>();
                                foreach (var (rotX, rotY) in rotation)
                                {
                                    matchedCells.Add((rotX + offsetX, rotY + offsetY));
                                }
                                return (template, matchedCells);
                            }
                        }
                    }
                }
            }
            return (null, null);
        }

        public static List<(ShapeTemplate template, HashSet<(int, int)> cells)> FindAllShapeMatchesInRegion(
            HashSet<(int, int)> region)
        {
            List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = new List<(ShapeTemplate, HashSet<(int, int)>)>();
            HashSet<(int, int)> remaining = new HashSet<(int, int)>(region);

            int maxIteration = region.Count;
            int iteration = 0;
            while (iteration < maxIteration)
            {
                var (template, matchedCells) = FindBestShapeMatchWithPosition(remaining);
                if (template == null)
                {
                    break;
                }
                
                matches.Add((template, matchedCells));
                foreach (var cell in matchedCells)
                {
                    remaining.Remove(cell);
                }
                
                if (remaining.Count == 0)
                {
                    break;
                }

                iteration++;
            }
            return  matches;
        }
        
        public static List<(Piece, int, int)> FindNearlyDamageable(Board board, HashSet<(int, int)> matchSet)
        {
            List<(Piece, int, int)> damageablePieces = new List<(Piece, int, int)>();
            (int dx, int dy)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            HashSet<Piece> addedPieces = new HashSet<Piece>();
            foreach (var (matchX, matchY)  in matchSet)
            {
                foreach (var (directionX, directionY) in directions)
                {
                    int newX = matchX + directionX;
                    int newY = matchY + directionY;

                    if (!BoardLogic.IsValidIndex(newX, newY, board))
                    {
                        continue;
                    }
                    
                    Piece piece = board.GetCell(newX, newY);
                    if (piece != null && piece.Flags.HasFlag(PieceFlags.DamageableByNeighbor) && !addedPieces.Contains(piece))
                    {
                        addedPieces.Add(piece);
                        damageablePieces.Add((piece, newX, newY));
                    }
                }
            }

            return damageablePieces;
        }

        public static List<OverlayPiece> FindNearlyDamageableOverlays(Board board, HashSet<(int, int)> matchSet)
        {
            List<OverlayPiece> damageableOverlays = new List<OverlayPiece>();
            (int damegableX, int damegableY)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

            foreach (var (matchX, matchY) in matchSet)
            {
                foreach (var (directionX, directionY) in directions)
                {
                    int newX = matchX + directionX;
                    int newY = matchY + directionY;

                    if (!BoardLogic.IsValidIndex(newX, newY, board))
                    {
                        continue;
                    }

                    OverlayPiece overlayPiece = board.GetOverlayPieceAt(newX, newY);
                    if (overlayPiece != null && overlayPiece.Flags.HasFlag(PieceFlags.DamageableByNeighbor) && !damageableOverlays.Contains(overlayPiece))
                    {
                        damageableOverlays.Add(overlayPiece);
                    }
                }
            }
            return  damageableOverlays;
        }

        public static RocketDirection ResolveRocketDirection(HashSet<(int, int)> matchedCells)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (var (x, y) in matchedCells)
            {
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
            bool isHorizontalMatch = (maxX - minX) > (maxY - minY);
            return isHorizontalMatch ? RocketDirection.Horizontal : RocketDirection.Vertical;
        }
    }

    public static class OrganizationLogic
    {
        public static (int pieceId, int toX, int toY)? FillOneDiagonalStep(Board board, IRandom random)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.GetCell(x, y) != null)
                    {
                        continue;
                    }

                    if (!HasBlockedPathToTop(board, x, y))
                    {
                        continue;
                    }

                    int firstDx = random.Next(2) == 0 ? -1 : 1;
                    int secondDx = -firstDx;

                    Piece moved = TryDiagonalMove(board, x, y, firstDx) ?? TryDiagonalMove(board, x, y, secondDx);
                    if (moved != null)
                    {
                        return (moved.Id, x, y);
                    }
                }
            }
            return null;
        }
        
        private static bool HasBlockedPathToTop(Board board, int x, int y)
        {
            for (int cy = y - 1; cy >= 0; cy--)
            {
                Piece piece = board.GetCell(x, cy);
                if (piece != null && (!BoardLogic.IsPieceMoveable(piece) || BoardLogic.IsPieceLocked(piece)))
                {
                    return true;
                }
            }
            return false;
        }
        
        private static Piece TryDiagonalMove(Board board, int targetX, int targetY, int dx)
        {
            int sourceX = targetX + dx;
            int sourceY = targetY - 1;
            if (!BoardLogic.IsValidIndex(sourceX, sourceY, board))
            {
                return null;
            }

            Piece source = board.GetCell(sourceX, sourceY);
            if (source == null || !BoardLogic.IsPieceMoveable(source) || BoardLogic.IsPieceLocked(source))
            {
                return null;
            }

            board.SetCell(targetX, targetY, source);
            board.ClearCell(sourceX, sourceY);
            return source;
        }
        
        public static List<(int, int, int)> FillEmpties(Board board)
        {
            List<(int, int, int)> collectedMovement = new List<(int, int, int)>();
            
            for(int x = 0; x < board.Width; x++)
            {
                int segmentBottom = board.Height - 1;
                List<(Piece piece, int y)> collected = new List<(Piece, int)>(); 
                for(int y = board.Height - 1; y >= 0; y--)
                {
                    Piece piece = board.GetCell(x, y);
                    if(piece != null)
                    {
                        if(!BoardLogic.IsPieceMoveable(piece) || BoardLogic.IsPieceLocked(piece))
                        {
                            CompactSegment(board, x, segmentBottom, y + 1, collected,
                                collectedMovement);
                            collected.Clear();
                            segmentBottom = y - 1;
                            continue;
                        }
                        collected.Add((piece, y));
                    }
                }
                CompactSegment(board, x, segmentBottom, 0, collected, collectedMovement);
            }
            return collectedMovement;
        }

        private static void CompactSegment(Board board, int x, int segmentBottom, int segmentTop,
            List<(Piece piece, int y)> collected, List<(int, int, int)> collectedMovement)
        {
            int writeY = segmentBottom;
            foreach (var (piece, y) in collected)
            {
                board.SetCell(x, writeY, piece);
                if(Math.Abs(y - writeY) > 0)
                    collectedMovement.Add((piece.Id, x, writeY));
                writeY--;
            }

            while (writeY >= segmentTop)
            {
                board.SetCell(x, writeY, null);
                writeY--;
            }
        }

        static T RandomEnumValue<T>(IRandom random)
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(random.Next(v.Length));
        }

        public static List<(int, PieceColor, int, int)> GetNewPieceToEmpty(Board board, IRandom random)
        {
            List<(int, PieceColor, int, int)> collectedMovement = new List<(int, PieceColor, int, int)>();
            for(int x = 0; x < board.Width; x++)
            {
                for(int y = 0; y < board.Height; y++)
                {
                    Piece piece = board.GetCell(x, y);
                    
                    if (piece != null && (!BoardLogic.IsPieceMoveable(piece) || BoardLogic.IsPieceLocked(piece)))
                    {
                        break;
                    }
                    
                    if(piece == null)
                    {
                        PieceColor newPieceColor = PickNonMatchingColor(board, x, y, random);
                        Piece newPiece = new ColoredPiece(board.NextId(), newPieceColor);
                        board.SetCell(x, y, newPiece);
                        collectedMovement.Add((newPiece.Id, newPiece.Color, x, y));
                    }
                }
            }
            return collectedMovement;
        }
        
        private static bool CreatesMatchAt(Board board, int x, int y, PieceColor color)
        {
            if (x >= 2)
            {
                Piece left1 = board.GetCell(x - 1, y);
                Piece left2 = board.GetCell(x - 2, y);
                if (left1 != null && left1.Color == color && left2 != null && left2.Color == color)
                {
                    return true;
                }
            }

            if (y >= 2)
            {
                Piece up1 = board.GetCell(x, y - 1);
                Piece up2 = board.GetCell(x, y - 2);
                if (up1 != null && up1.Color == color && up2 != null && up2.Color == color)
                {
                    return true;
                }
            }

            return false;
        }

        private static PieceColor PickNonMatchingColor(Board board, int x, int y, IRandom random)
        {
            const int maxAttempts = 20;
            PieceColor color = PieceColor.Empty;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                PieceColor candidate = PieceColor.Empty;
                while (candidate == PieceColor.Empty)
                {
                    candidate = RandomEnumValue<PieceColor>(random);
                }

                color = candidate;
                if (!CreatesMatchAt(board, x, y, candidate))
                {
                    break;
                }
            }
            return color;
        }
    }
    
    public static class SpecialActivationLogic
    {
        public struct ActivationEffect
        {
            public HashSet<(int x, int y)> Cells;
            public (int x, int y)? FlightTarget;
            public List<(int x, int y)> NeighborCells;
        }
        
        public static ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority = null)
        {
            var cells = new HashSet<(int x, int y)>();

            if (piece is RocketPiece rocket)
            {
                if (rocket.Direction == RocketDirection.Horizontal)
                {
                    for (int cx = 0; cx < board.Width; cx++)
                    {
                        cells.Add((cx, y));
                    }
                }
                else
                {
                    for (int cy = 0; cy < board.Height; cy++)
                    {
                        cells.Add((x, cy));
                    }
                }
                return new ActivationEffect { Cells = cells, FlightTarget = null };
            }

            if (piece is SpecialPiece sp)
            {
                if (sp.Kind == PieceKind.Tnt)
                {
                    for (int cx = x - 1; cx <= x + 1; cx++)
                    {
                        for (int cy = y - 1; cy <= y + 1; cy++)
                        {
                            if (cx < 0 || cx >= board.Width || cy < 0 || cy >= board.Height) continue;
                            {
                                cells.Add((cx, cy));
                            }
                        }
                    }
                    return new ActivationEffect { Cells = cells, FlightTarget = null };
                }

                if (sp.Kind == PieceKind.Propeller)
                {
                    var (targetX, targetY) = PickTarget(board, random, targetPriority);
                    cells.Add((targetX, targetY));
                    cells.Add((x, y));
    
                    var neighborCells = AddNeighbors(board, x, y, cells);
                    return new ActivationEffect { Cells = cells, FlightTarget = (targetX, targetY), NeighborCells = neighborCells };
                }

                if (sp.Kind == PieceKind.LightBall)
                {
                    PieceColor targetColor = (partner != null && partner.VisualType == PieceVisualType.Standard && partner.Color != PieceColor.Empty)
                        ? partner.Color
                        : GetMostCommonColor(board);

                    if (targetColor != PieceColor.Empty)
                    {
                        for (int cx = 0; cx < board.Width; cx++)
                        {
                            for (int cy = 0; cy < board.Height; cy++)
                            {
                                Piece p = board.GetCell(cx, cy);
                                if (p != null && p.VisualType == PieceVisualType.Standard && p.Color == targetColor)
                                    cells.Add((cx, cy));
                            }
                        }
                    }
                    cells.Add((x, y));
                    return new ActivationEffect { Cells = cells, FlightTarget = null };
                }
            }

            cells.Add((x, y));
            return new ActivationEffect { Cells = cells, FlightTarget = null };
        }

        public static HashSet<(int x, int y)> GetComboAffectedCells(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority = null)
        {
            var cells = new HashSet<(int x, int y)>();

            if (IsPair(kindA, kindB, PieceKind.Rocket, PieceKind.Rocket))
            {
                AddRowAndColumn(board, x, y, cells);
            }
            else if (IsPair(kindA, kindB, PieceKind.Rocket, PieceKind.Tnt))
            {
                AddThickCross(board, x, y, 1, cells);
            }
            else if (IsPair(kindA, kindB, PieceKind.Tnt, PieceKind.Tnt))
            {
                AddArea(board, x, y, 4, cells);
            }
            else if (IsPair(kindA, kindB, PieceKind.Rocket, PieceKind.Propeller))
            {
                var (rx, ry) = PickTarget(board, random, targetPriority);
                AddDirectional(board, rx, ry, rocketDirection ?? RocketDirection.Horizontal, cells);
            }
            else if (IsPair(kindA, kindB, PieceKind.Tnt, PieceKind.Propeller))
            {
                var (rx, ry) = PickTarget(board, random, targetPriority);
                AddArea(board, rx, ry, 1, cells);
            }
            else if (IsPair(kindA, kindB, PieceKind.Propeller, PieceKind.Propeller))
            {
                for (int i = 0; i < 3; i++)
                {
                    cells.Add(PickTarget(board, random, targetPriority));
                }            
            }
            else if (IsPair(kindA, kindB, PieceKind.LightBall, PieceKind.LightBall))
            {
                for (int cx = 0; cx < board.Width; cx++)
                {
                    for (int cy = 0; cy < board.Height; cy++)
                    {
                        cells.Add((cx, cy));
                    }
                }
            }
            
            cells.Add((x, y));
            return cells;
        }
        
        private static (int x, int y) PickTarget(Board board, IRandom random, ITargetPriority targetPriority)
        {
            if (targetPriority != null)
            {
                List<(int x, int y)> bestCells = new List<(int x, int y)>();
                int bestPriority = 0;

                for (int cx = 0; cx < board.Width; cx++)
                {
                    for (int cy = 0; cy < board.Height; cy++)
                    {
                        int priority = targetPriority.GetPriority(board, cx, cy);
                        if (priority <= 0)
                        {
                            continue;
                        }
                        if (priority > bestPriority)
                        {
                            bestPriority = priority;
                            bestCells.Clear();
                        }
                        if (priority == bestPriority)
                        {
                            bestCells.Add((cx, cy));
                        }
                    }
                }

                if (bestCells.Count > 0)
                {
                    return bestCells[random.Next(bestCells.Count)];
                }
            }
            return RandomCell(board, random);
        }
        
        private static bool IsPair(PieceKind a, PieceKind b, PieceKind x, PieceKind y)
        {
            return (a == x && b == y) || (a == y && b == x);
        }
        
        private static void AddRowAndColumn(Board board, int x, int y, HashSet<(int x, int y)> cells)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                cells.Add((cx, y));
            }
            for (int cy = 0; cy < board.Height; cy++)
            {
                cells.Add((x, cy));
            }
        }
        
        private static void AddArea(Board board, int x, int y, int radius, HashSet<(int x, int y)> cells)
        {
            for (int cx = x - radius; cx <= x + radius; cx++)
            {
                for (int cy = y - radius; cy <= y + radius; cy++)
                {
                    if (cx < 0 || cx >= board.Width || cy < 0 || cy >= board.Height)
                    {
                        continue;
                    }
                    cells.Add((cx, cy));
                }
            }
        }
        
        private static void AddThickCross(Board board, int x, int y, int thickness, HashSet<(int x, int y)> cells)
        {
            for (int oy = -thickness; oy <= thickness; oy++)
            {
                int ry = y + oy;
                if (ry < 0 || ry >= board.Height)
                {
                    continue;
                }
                for (int cx = 0; cx < board.Width; cx++)
                {
                    cells.Add((cx, ry));
                }
            }
            for (int ox = -thickness; ox <= thickness; ox++)
            {
                int rx = x + ox;
                if (rx < 0 || rx >= board.Width)
                {
                    continue;
                }
                for (int cy = 0; cy < board.Height; cy++)
                {
                    cells.Add((rx, cy));
                }
            }
        }
        
        private static List<(int x, int y)> AddNeighbors(Board board, int x, int y, HashSet<(int x, int y)> cells)
        {
            var neighbors = new List<(int x, int y)>();
            (int dx, int dy)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dx, dy) in directions)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || nx >= board.Width || ny < 0 || ny >= board.Height)
                {
                    continue;
                }
                cells.Add((nx, ny));
                neighbors.Add((nx, ny));
            }
            return neighbors;
        }
        
        private static void AddDirectional(Board board, int x, int y, RocketDirection direction, HashSet<(int x, int y)> cells)
        {
            if (direction == RocketDirection.Horizontal)
            {
                for (int cx = 0; cx < board.Width; cx++)
                {
                    cells.Add((cx, y));
                }
            }
            else
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    cells.Add((x, cy));
                }
            }
        }
        
        private static (int x, int y) RandomCell(Board board, IRandom random)
        {
            return (random.Next(board.Width), random.Next(board.Height));
        }
        
        private static PieceColor GetMostCommonColor(Board board)
        {
            var counts = new Dictionary<PieceColor, int>();
            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    Piece p = board.GetCell(cx, cy);
                    if (p == null || p.VisualType != PieceVisualType.Standard)
                    {
                        continue;
                    }

                    counts.TryGetValue(p.Color, out int count);
                    counts[p.Color] = count + 1;
                }
            }

            PieceColor best = PieceColor.Empty;
            int bestCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > bestCount)
                {
                    best = kvp.Key;
                    bestCount = kvp.Value;
                }
            }
            return best;
        }
        
        public static List<(Piece oldPiece, Piece newPiece, int x, int y)> TransformMostCommonColorToKind(Board board, PieceKind kind, IRandom random)
        {
            var result = new List<(Piece oldPiece, Piece newPiece, int x, int y)>();
            PieceColor color = GetMostCommonColor(board);
            if (color == PieceColor.Empty)
            {
                return result;
            }

            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    Piece p = board.GetCell(cx, cy);
                    if (p == null || p.VisualType != PieceVisualType.Standard || p.Color != color)
                    {
                        continue;
                    }

                    Piece newPiece = CreateSpecial(board, kind, random);
                    board.SetCell(cx, cy, newPiece);
                    result.Add((p, newPiece, cx, cy));
                }
            }

            return result;
        }
        
        private static Piece CreateSpecial(Board board, PieceKind kind, IRandom random)
        {
            if (kind == PieceKind.Rocket)
            {
                RocketDirection direction = random.Next(2) == 0 ? RocketDirection.Horizontal : RocketDirection.Vertical;
                return new RocketPiece(board.NextId(), direction);
            }
            return new SpecialPiece(board.NextId(), kind);
        }
    }
    
    public static class MoveLogic
    {
        public static bool HasAvailableMove(Board board)
        {
            return FindAvailableMove(board) != null;
        }
        
        public static (int x1, int y1, int x2, int y2)? FindAvailableMove(Board board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (TryNeighborSwap(board, x, y, x + 1, y))
                    {
                        return (x, y, x + 1, y);
                    }
                    if (TryNeighborSwap(board, x, y, x, y + 1))
                    {
                        return (x, y, x, y + 1);
                    }
                }
            }
            return null;
        }

        private static bool TryNeighborSwap(Board board, int x1, int y1, int x2, int y2)
        {
            if (!BoardLogic.IsValidIndex(x2, y2, board))
            {
                return false;
            }

            Piece p1 = board.GetCell(x1, y1);
            Piece p2 = board.GetCell(x2, y2);
            if (p1 == null || p2 == null)
            {
                return false;
            }
            if (BoardLogic.IsCellSwapBlocked(board, x1, y1) || BoardLogic.IsCellSwapBlocked(board, x2, y2))
            {
                return false;
            }
            if (!BoardLogic.IsPieceMoveable(p1) || !BoardLogic.IsPieceMoveable(p2))
            {
                return false;
            }

            if (p1.VisualType == PieceVisualType.Special || p2.VisualType == PieceVisualType.Special)
            {
                return true;
            }

            board.SetCell(x1, y1, p2);
            board.SetCell(x2, y2, p1);

            bool producesMatch = MatchLogic.FindMatches(board).Count > 0;

            board.SetCell(x1, y1, p1);
            board.SetCell(x2, y2, p2);

            return producesMatch;
        }
    }
    
    public static class ShuffleLogic
    {
        private const int MaxAttempts = 80;

        public static List<(int pieceId, int toX, int toY)> Shuffle(Board board, IRandom random)
        {
            List<(int x, int y)> positions = new List<(int, int)>();
            List<Piece> pieces = new List<Piece>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Piece piece = board.GetCell(x, y);
                    if (piece != null && BoardLogic.IsPieceMoveable(piece) && !BoardLogic.IsPieceLocked(piece))
                    {
                        positions.Add((x, y));
                        pieces.Add(piece);
                    }
                }
            }

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                ShuffleListInPlace(pieces, random);

                for (int i = 0; i < positions.Count; i++)
                {
                    board.SetCell(positions[i].x, positions[i].y, pieces[i]);
                }

                bool createsMatch = MatchLogic.FindMatches(board).Count > 0;
                bool hasMove = MoveLogic.HasAvailableMove(board);

                if (!createsMatch && hasMove)
                {
                    break;
                }
            }

            var moves = new List<(int pieceId, int toX, int toY)>();
            for (int i = 0; i < positions.Count; i++)
            {
                moves.Add((pieces[i].Id, positions[i].x, positions[i].y));
            }
            return moves;
        }

        private static void ShuffleListInPlace(List<Piece> pieces, IRandom random)
        {
            for (int i = pieces.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }
        }
    }
}
