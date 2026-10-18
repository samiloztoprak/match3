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
    }

    public static class MatchLogic
    {
        private const int MinMatchLength = 3; 

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

                    if(piece == null || !piece.Flags.HasFlag(PieceFlags.CanMatch))
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
            
            foreach (var match in matchSet)
            {
                foreach (var (directionX, directionY) in directions)
                {
                    int newX = match.Item1 + directionX;
                    int newY = match.Item2 + directionY;

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
                        if(!piece.Flags.HasFlag(PieceFlags.Movable))
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

                    if (piece != null && !piece.Flags.HasFlag(PieceFlags.Movable))
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
