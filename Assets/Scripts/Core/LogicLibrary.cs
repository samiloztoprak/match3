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
                if(piece.Type != pieceType)
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
                if(piece.Type != pieceType)
                {
                    return i-1;
                }
            }
            return board.Height-1;
        }
    }
}
