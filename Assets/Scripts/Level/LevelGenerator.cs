using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GeneratorPiece
{
    public LevelLayer layer;
    public string code;
    public int count;
}

[Serializable]
public class LevelGeneratorSettings
{
    public int width = 8;
    public int height = 8;
    public int colorCount = 4;
    public int moves = 30;
    public bool mirror = true;
    public int colorGoalCount;
    public List<GeneratorPiece> pieces = new List<GeneratorPiece>();
}

public static class LevelGenerator
{
    private static readonly string[] ColorOrder = { "Red", "Yellow", "Blue", "Green" };
    private static readonly LevelLayer[] LayerOrder = { LevelLayer.Cell, LevelLayer.Floor, LevelLayer.Overlay };
    private const int TriesPerPiece = 40;

    public static LevelData Generate(LevelGeneratorSettings settings, int seed)
    {
        var random = new SystemRandom(seed);
        int colorCount = Math.Max(2, Math.Min(ColorOrder.Length, settings.colorCount));

        var level = new LevelData
        {
            width = settings.width,
            height = settings.height,
            moves = settings.moves,
            colors = ColorOrder.Take(colorCount).ToArray(),
        };
        LevelEditing.EnsureRows(level);

        foreach (LevelLayer layer in LayerOrder)
        {
            foreach (GeneratorPiece piece in settings.pieces)
            {
                if (piece.layer != layer || piece.count <= 0 || string.IsNullOrEmpty(piece.code))
                {
                    continue;
                }

                ILevelElement element = LevelElements.Get(layer, piece.code[0]);
                if (element != null)
                {
                    Place(level, element, piece.count, settings.mirror, random);
                }
            }
        }

        if (settings.colorGoalCount > 0)
        {
            List<PieceColor> colors = LevelValidator.ParseColors(level.colors);
            LevelEditing.AddColorGoal(level, colors[random.Next(colors.Count)], settings.colorGoalCount);
        }
        return level;
    }
    
    private static int Place(LevelData level, ILevelElement element, int count, bool mirror, IRandom random)
    {
        int width = Footprint(element);
        int columns = mirror ? (level.width + 1) / 2 : level.width;
        int placed = 0;

        for (int tries = 0; placed < count && tries < count * TriesPerPiece; tries++)
        {
            int x = random.Next(columns);
            int y = random.Next(level.height);
            if (level.At(element.Layer, x, y) != LevelData.Empty)
            {
                continue;
            }
            if (!LevelEditing.Paint(level, element, x, y, out _))
            {
                continue;
            }
            placed++;

            int twin = level.width - x - width;
            if (mirror && twin > x && placed < count && level.At(element.Layer, twin, y) == LevelData.Empty && LevelEditing.Paint(level, element, twin, y, out _))
            {
                placed++;
            }
        }
        return placed;
    }

    private static int Footprint(ILevelElement element)
    {
        var board = new Board(4, 4);
        element.Place(board, 0, 0);
        if (element.Layer != LevelLayer.Overlay || board.OverlayPieces.Count == 0)
        {
            return 1;
        }
        return board.OverlayPieces[0].CoveredCells.Max(c => c.x) + 1;
    }
}