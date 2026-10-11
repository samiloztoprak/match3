using System;

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
