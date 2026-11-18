using System;
using System.Collections.Generic;

public static class LevelValidator
{
    public const int MinSize = 3;
    public const int MaxSize = 10;
    private static readonly LevelLayer[] Layers =
    {
        LevelLayer.Cell, LevelLayer.Overlay, LevelLayer.Floor
    };

    public static List<string> Validate(LevelData level)
    {
        var errors = new List<string>();
        if (level == null)
        {
            errors.Add("Level is null."); return errors;
        }

        if (level.width < MinSize || level.width > MaxSize || level.height < MinSize || level.height > MaxSize)
        {
            errors.Add($"Size must be between {MinSize} and {MaxSize}.");
        }
        
        if (level.moves <= 0)
        {
            errors.Add("Moves must be greater than 0.");
        }

        List<PieceColor> colors = ParseColors(level.colors, errors);
        CheckRows(level, LevelLayer.Cell, errors, required: true);
        CheckRows(level, LevelLayer.Overlay, errors, required: false);
        CheckRows(level, LevelLayer.Floor, errors, required: false);
        
        if (errors.Count > 0)
        {
            return errors;
        }

        int playable = 0;
        var autoGoalCounts = new Dictionary<GoalType, int>();

        foreach (LevelLayer layer in Layers)
        {
            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    char code = level.At(layer, x, y);
                    if (layer == LevelLayer.Cell && code != '#')
                    {
                        playable++;
                    }
                    
                    if (code == LevelData.Empty)
                    {
                        continue;
                    }

                    ILevelElement element = LevelElements.Get(layer, code);
                    if (element == null)
                    {
                        errors.Add($"Unknown {layer} code '{code}' at ({x},{y}).");
                        continue;
                    }

                    if (!element.CanPlace(level, x, y, out string reason))
                    {
                        errors.Add($"{element.DisplayName} at ({x},{y}): {reason}");
                    }

                    if (element.AutoGoal.HasValue)
                    {
                        autoGoalCounts.TryGetValue(element.AutoGoal.Value, out int n);
                        autoGoalCounts[element.AutoGoal.Value] = n + 1;
                    }
                }
            }
        }

        if (playable == 0) errors.Add("The board has no playable cells.");

        foreach (LevelGoalData goal in level.goals)
        {
            if (!Enum.TryParse(goal.type, out GoalType type))
            {
                errors.Add($"Unknown goal type '{goal.type}'.");
                continue;
            }
            
            if (goal.count <= 0)
            {
                errors.Add($"{goal.type} needs a count greater than 0.");
            }

            if (type == GoalType.CollectColor)
            {
                if (!Enum.TryParse(goal.color, out PieceColor color) || !colors.Contains(color))
                {
                    errors.Add($"CollectColor uses a color that isn't in the level: '{goal.color}'.");
                }
                continue;
            }

            autoGoalCounts.TryGetValue(type, out int onBoard);
            if (goal.count > onBoard)
            {
                errors.Add($"{goal.type} needs {goal.count} but the board has {onBoard}.");
            }
        }

        return errors;
    }

    public static List<PieceColor> ParseColors(string[] names, List<string> errors = null)
    {
        var list = new List<PieceColor>();
        if (names != null)
        {
            foreach (string name in names)
            {
                if (Enum.TryParse(name, out PieceColor c) && c != PieceColor.Empty && !list.Contains(c))
                {
                    list.Add(c);
                }
                else
                {
                    errors?.Add($"Invalid or duplicate color '{name}'.");
                }
            }
        }
        
        if (list.Count < 2)
        {
            errors?.Add("A level needs at least 2 colors.");
        }
        return list;
    }

    private static void CheckRows(LevelData level, LevelLayer layer, List<string> errors, bool required)
    {
        string[] rows = level.Rows(layer);
        if (rows == null || rows.Length == 0)
        {
            if (required)
            {
                errors.Add($"'{layer}' rows are missing.");
            }
            return;
        }
        
        if (rows.Length != level.height)
        {
            errors.Add($"'{layer}' has {rows.Length} rows, expected {level.height}.");
        }
        
        for (int y = 0; y < rows.Length; y++)
        {
            if (rows[y] == null || rows[y].Length != level.width)
            {
                errors.Add($"'{layer}' row {y} has length {rows[y]?.Length ?? 0}, expected {level.width}.");
            }
        }
    }
}