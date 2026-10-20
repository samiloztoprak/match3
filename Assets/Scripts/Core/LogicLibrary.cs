using System;
using System.Collections.Generic;
using NUnit.Framework;

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
        private const int MinMatchLength = 3;

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

        public static ShapeTemplate FindBestShapeMatch(HashSet<(int, int)> clusterCells)
        {
            HashSet<(int, int)> normalizedCluster = ShapeTemplate.Normalize(clusterCells);
            ShapeTemplate bestMatch = null;

            foreach (var template in ShapeRegistry.Templates)
            {
                foreach (var rotation in template.GetAllRotations())
                {
                    if (rotation.Count != normalizedCluster.Count)
                    {
                        continue;
                    }

                    if (rotation.SetEquals(normalizedCluster))
                    {
                        if (bestMatch == null || template.Priority > bestMatch.Priority)
                        {
                            bestMatch = template;
                        }
                        break;
                    }
                }
            }

            return bestMatch;
        }
        
        public static HashSet<(int, int)> FindMatches(Board board)
        {
            HashSet<(int, int)> matchSet = new HashSet<(int, int)>();

            for(int y = 0; y < board.Height; y++)
            {
                for(int x = 0; x < board.Width; x++)
                {
                    Piece piece = board.GetCell(x, y);

                    if(piece == null || !piece.Flags.HasFlag(PieceFlags.CanMatch))
                    {
                        continue;
                    }

                    int rleResult = RunRLEHorizontal(x, y, piece, board);
                    int runLength = CalculateRunLength(rleResult, x);
                    if(runLength >= MinMatchLength)
                    {
                        for(int xMatch = x; xMatch <= rleResult; xMatch++)
                        {
                            matchSet.Add((xMatch, y));
                        }
                        x = rleResult+1;
                    }
                }
            }
            
            for(int x = 0; x < board.Width; x++)
            {
                for(int y = 0; y < board.Height; y++)
                {
                    Piece piece = board.GetCell(x, y);

                    if(piece == null || !piece.Flags.HasFlag(PieceFlags.CanMatch) 
                    || piece.Flags.HasFlag(PieceFlags.Locked))
                    {
                        continue;
                    }

                    int rleResult = RunRLEVertical(x, y, piece, board);
                    int runLength = CalculateRunLength(rleResult, y);
                    if(runLength >= MinMatchLength)
                    {
                        for(int yMatch = y; yMatch <= rleResult; yMatch++)
                        {
                            matchSet.Add((x, yMatch));
                        }
                        y = rleResult+1;
                    }
                }
            }

            return matchSet;
        }

        public static int CalculateRunLength(int index1, int index2)
        {
            return index1 - index2 + 1;
        }

        //Run Length Encoding
        public static int RunRLEHorizontal(int x, int y, Piece piece, Board board)
        {
            for(int i = x; i < board.Width; i++)
            {
                Piece _piece = board.GetCell(i, y);
                if(_piece == null || !_piece.Flags.HasFlag(PieceFlags.CanMatch) || _piece.Color != piece.Color)
                {
                    return i-1;
                }
            }
            return board.Width-1;
        }

        public static int RunRLEVertical(int x, int y, Piece piece, Board board)
        {
            for(int i = y; i < board.Height; i++)
            {
                Piece _piece = board.GetCell(x, i);
                if(_piece == null || !_piece.Flags.HasFlag(PieceFlags.CanMatch) || _piece.Color != piece.Color)
                {
                    return i-1;
                }
            }
            return board.Height-1;
        }

        public static List<(Piece, int, int)> FindNearlyDamageable(Board board, HashSet<(int, int)> matchSet)
        {
            List<(Piece, int, int)> damageablePieces = new List<(Piece, int, int)>();
            (int dx, int dy)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            
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
                    if (piece != null && piece.Flags.HasFlag(PieceFlags.DamageableByNeighbor))
                    {
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
                    if (overlayPiece != null && overlayPiece.Flags.HasFlag(PieceFlags.DamageableByNeighbor))
                    {
                        damageableOverlays.Add(overlayPiece);
                    }
                }
            }
            return  damageableOverlays;
        }
    }

    public static class OrganizationLogic
    {
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
        
        static Random _R = new Random ();
        static T RandomEnumValue<T> ()
        {
            var v = Enum.GetValues (typeof (T));
            return (T) v.GetValue (_R.Next(v.Length));
        }

        public static List<(int, PieceColor, int, int)> GetNewPieceToEmpty(Board board)
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
                        PieceColor newPieceColor = PieceColor.Empty;
                        while(newPieceColor == PieceColor.Empty)
                        {
                            newPieceColor = RandomEnumValue<PieceColor>();
                        }
                        Piece newPiece = new ColoredPiece(board.NextId(), newPieceColor);
                        board.SetCell(x, y, newPiece);
                        collectedMovement.Add((newPiece.Id, newPiece.Color, x, y));
                    }
                }
            }
            return collectedMovement;
        }
    }
}
