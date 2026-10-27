
using System;
using System.Collections.Generic;

public sealed class ShapeTemplate
{
    public string Name { get; }
    public int Priority { get; }
    public HashSet<(int, int)> Cells { get; }
    public List<HashSet<(int, int)>> Rotations { get; }
    public List<(int maxX, int maxY)> RotationBounds { get; }
    
    public ShapeTemplate(string name, int priority, bool[,] matrix)
    {
        Name = name.ToLower();
        Priority = priority;
        Cells = Normalize(MatrixToCells(matrix));
        Rotations = ComputeAllRotations();
        RotationBounds = ComputeRotationBounds();
    }

    private static HashSet<(int, int)> MatrixToCells(bool[,] matrix)
    {
        HashSet<(int, int)> cells = new HashSet<(int, int)>();
        int height = matrix.GetLength(0);
        int width = matrix.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (matrix[y, x])
                {
                    cells.Add((x, y));
                }
            }
        }
        return cells;
    }

    public static HashSet<(int, int)> Normalize(HashSet<(int, int)> cells)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (var (x, y) in cells)
        {
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
        }

        HashSet<(int, int)> normalized = new HashSet<(int, int)>();
        foreach (var (x, y) in cells)
        {
            normalized.Add((x - minX, y - minY));
        }
        return normalized;
    }

    private static HashSet<(int, int)> Rotate90(HashSet<(int, int)> cells)
    {
        HashSet<(int, int)> rotated = new HashSet<(int, int)>();
        foreach (var (x, y) in cells)
        {
            rotated.Add((-y, x));
        }
        return Normalize(rotated);
    }

    public List<HashSet<(int, int)>> ComputeAllRotations()
    {
        List<HashSet<(int, int)>> rotations = new List<HashSet<(int, int)>>();
        HashSet<(int, int)> current = Cells;

        for (int i = 0; i < 4; i++)
        {
            rotations.Add(current);
            current = Rotate90(current);
        }
        return rotations;
    }

    private List<(int maxX, int maxY)> ComputeRotationBounds()
    {
        List<(int maxX, int maxY)> bounds = new List<(int, int)>();

        foreach (var rotation in Rotations)
        {
            int maxX = 0;
            int maxY = 0;
            foreach (var (x, y) in rotation)
            {
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            bounds.Add((maxX, maxY));
        }
        return bounds;
    }
    
}
