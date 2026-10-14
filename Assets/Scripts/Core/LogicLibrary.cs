using System;
using System.Collections.Generic;

namespace LogicLibrary
{
    public static class BoardLogic
    {
        public static bool IsSwappableIndices(int x1, int y1, int x2, int y2)
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

                    if(piece.Type == PieceType.Empty)
                    {
                        continue;
                    }

                    int rleResult = RunRLEHorizontal(x, y, piece.Type, board);
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

                    if(piece.Type == PieceType.Empty)
                    {
                        continue;
                    }

                    int rleResult = RunRLEVertical(x, y, piece.Type, board);
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
        public static int RunRLEHorizontal(int x, int y, PieceType pieceType, Board board)
        {
            for(int i = x; i < board.Width; i++)
            {
                Piece piece = board.GetCell(i, y);
                if(piece.Type == PieceType.Empty || piece.Type != pieceType)
                {
                    return i-1;
                }
            }
            return board.Width-1;
        }

        public static int RunRLEVertical(int x, int y, PieceType pieceType, Board board)
        {
            for(int i = y; i < board.Height; i++)
            {
                Piece piece = board.GetCell(x, i);
                if(piece.Type == PieceType.Empty || piece.Type != pieceType)
                {
                    return i-1;
                }
            }
            return board.Height-1;
        }
    }

    public static class OrganizationLogic
    {
        public static List<(int, int, int, int)> FillEmpties(Board board)
        {
            List<(int, int, int, int)> collectedMovement = new List<(int, int, int, int)>();
            for(int x = 0; x < board.Width; x++)
            {
                List<(Piece, int)> collected = new List<(Piece, int)>(); 
                for(int y = board.Height - 1; y >= 0; y--)
                {
                    Piece piece = board.GetCell(x, y);
                    if(piece.Type != PieceType.Empty)
                    {
                        collected.Add((piece, y));
                    }
                }

                int writeY = board.Height - 1;
                foreach (var (piece, y) in collected)
                {
                    board.SetCell(x, writeY, piece);
                    if(Math.Abs(y - writeY) > 0)
                        collectedMovement.Add((x, y, x, writeY));
                    writeY--;
                }

                while (writeY >= 0)
                {
                    board.SetCell(x, writeY, new Piece(PieceType.Empty));
                    writeY--;
                }
            }
            return collectedMovement;
        }
    }
}
