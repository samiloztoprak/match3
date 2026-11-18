using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public const char Empty = '.';

    public int version = 1;
    public int width = 7;
    public int height = 7;
    public int moves = 20;
    public string[] colors = { "Red", "Yellow", "Blue", "Green" };
    public LevelStars stars = new LevelStars();
    public string[] cells;
    public string[] overlays;
    public string[] floors;
    public List<LevelGoalData> goals = new List<LevelGoalData>();

    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    public char At(LevelLayer layer, int x, int y) => CharAt(Rows(layer), x, y);

    public string[] Rows(LevelLayer layer)
    {
        switch (layer)
        {
            case LevelLayer.Cell: return cells;
            case LevelLayer.Overlay: return overlays;
            default: return floors;
        }
    }

    public bool TryGetCage(int x, int y, out int anchorX, out int anchorY)
    {
        for (int dy = 0; dy <= 1; dy++)
        for (int dx = 0; dx <= 1; dx++)
        {
            anchorX = x - dx;
            anchorY = y - dy;
            if (InBounds(anchorX, anchorY) && At(LevelLayer.Overlay, anchorX, anchorY) == CageElement.CageCode)
            {
                return true;
            }
        }
        anchorX = anchorY = -1;
        return false;
    }

    public int Count(LevelLayer layer, char code)
    {
        int count = 0;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (At(layer, x, y) == code) count++;
        }
        return count;
    }

    private static char CharAt(string[] rows, int x, int y)
    {
        if (rows == null || y < 0 || y >= rows.Length) return Empty;
        string row = rows[y];
        return row != null && x >= 0 && x < row.Length ? row[x] : Empty;
    }
}

[Serializable]
public class LevelStars
{
    public int two;
    public int three;
}

[Serializable]
public class LevelGoalData
{
    public string type;
    public string color;
    public int count;
}