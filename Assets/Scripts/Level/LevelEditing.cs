using System;
using System.Collections.Generic;
using System.Linq;

public static class LevelEditing
{
    private static readonly LevelLayer[] Layers = { LevelLayer.Cell, LevelLayer.Overlay, LevelLayer.Floor };
    
    public static Dictionary<GoalType, int> CountAutoGoals(LevelData level)
    {
        var counts = new Dictionary<GoalType, int>();
        foreach (LevelLayer layer in Layers)
        {
            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    char code = level.At(layer, x, y);
                    if (code == LevelData.Empty) continue;

                    GoalType? goal = LevelElements.Get(layer, code)?.AutoGoal;
                    if (!goal.HasValue) continue;

                    counts.TryGetValue(goal.Value, out int n);
                    counts[goal.Value] = n + 1;
                }
            }
        }
        return counts;
    }
    
    public static void SyncAutoGoals(LevelData level, Dictionary<GoalType, int> before)
    {
        Dictionary<GoalType, int> after = CountAutoGoals(level);
        var types = new HashSet<GoalType>(before.Keys);
        types.UnionWith(after.Keys);

        foreach (GoalType type in types)
        {
            before.TryGetValue(type, out int oldTotal);
            after.TryGetValue(type, out int newTotal);
            LevelGoalData goal = level.goals.Find(g => g.type == type.ToString());

            if (newTotal == 0)
            {
                if (goal != null) level.goals.Remove(goal);
            }
            else if (goal == null)
            {
                level.goals.Add(new LevelGoalData { type = type.ToString(), count = newTotal });
            }
            else
            {
                goal.count = goal.count == oldTotal ? newTotal : Math.Min(goal.count, newTotal);
            }
        }
    }
    
    public static void EnsureRows(LevelData level)
    {
        level.cells = Fit(level.cells, level.width, level.height);
        level.overlays = Fit(level.overlays, level.width, level.height);
        level.floors = Fit(level.floors, level.width, level.height);
    }

    public static void Resize(LevelData level, int width, int height)
    {
        var before = CountAutoGoals(level);
        level.width = width;
        level.height = height;
        EnsureRows(level);
        RemoveOverlaysThatDontFit(level);
        SyncAutoGoals(level, before);
    }

    public static bool Paint(LevelData level, ILevelElement element, int x, int y, out string reason)
    {
        if (!level.InBounds(x, y))
        {
            reason = "Outside the board.";
            return false;
        }
        if (!element.CanPlace(level, x, y, out reason))
        {
            return false;
        }
        var before = CountAutoGoals(level);
        SetCode(level, element.Layer, x, y, element.Code);
        SyncAutoGoals(level, before);
        return true;
    }

    public static bool Erase(LevelData level, LevelLayer layer, int x, int y)
    {
        if (!level.InBounds(x, y)) return false;

        if (layer == LevelLayer.Overlay && level.At(layer, x, y) == LevelData.Empty
            && level.TryGetCage(x, y, out int anchorX, out int anchorY))
        {
            x = anchorX;
            y = anchorY;
        }

        if (level.At(layer, x, y) == LevelData.Empty) return false;
        var before = CountAutoGoals(level);
        SetCode(level, layer, x, y, LevelData.Empty);
        SyncAutoGoals(level, before);
        return true;
    }

    public static void SetCode(LevelData level, LevelLayer layer, int x, int y, char code)
    {
        string[] rows = level.Rows(layer);
        char[] row = rows[y].ToCharArray();
        row[x] = code;
        rows[y] = new string(row);
    }

    private static void RemoveOverlaysThatDontFit(LevelData level)
    {
        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                char code = level.At(LevelLayer.Overlay, x, y);
                if (code == LevelData.Empty)
                {
                    continue;
                }

                ILevelElement element = LevelElements.Get(LevelLayer.Overlay, code);
                if (element != null && !element.CanPlace(level, x, y, out _))
                {
                    SetCode(level, LevelLayer.Overlay, x, y, LevelData.Empty);
                }
            }
        }
    }

    private static string[] Fit(string[] rows, int width, int height)
    {
        var result = new string[height];
        for (int y = 0; y < height; y++)
        {
            string old = rows != null && y < rows.Length && rows[y] != null ? rows[y] : "";
            result[y] = old.Length >= width
                ? old.Substring(0, width)
                : old + new string(LevelData.Empty, width - old.Length);
        }
        return result;
    }
    
    public static void SetColors(LevelData level, IReadOnlyCollection<PieceColor> colors)
    {
        level.colors = colors.Select(c => c.ToString()).ToArray();

        foreach (ColorElement element in LevelElements.ByLayer(LevelLayer.Cell).OfType<ColorElement>())
        {
            if (colors.Contains(element.Color)) continue;
            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    if (level.At(LevelLayer.Cell, x, y) == element.Code) SetCode(level, LevelLayer.Cell, x, y, LevelData.Empty);
                }
            }
        }

        level.goals.RemoveAll(g => g.type == nameof(GoalType.CollectColor)
                                   && !colors.Any(c => c.ToString() == g.color));
    }

    public static bool AddColorGoal(LevelData level, PieceColor color, int count)
    {
        string name = color.ToString();
        if (level.goals.Exists(g => g.type == nameof(GoalType.CollectColor) && g.color == name)) return false;

        level.goals.Add(new LevelGoalData { type = nameof(GoalType.CollectColor), color = name, count = Math.Max(1, count) });
        return true;
    }
}