using System;
using System.Collections.Generic;
using LogicLibrary;

public static class LevelBoardBuilder
{
    public static Board Build(LevelData level, IRandom random)
    {
        var board = new Board(level.width, level.height);
        board.SetColors(LevelValidator.ParseColors(level.colors));

        PlaceLayer(board, level, LevelLayer.Cell);
        OrganizationLogic.FillAllEmptyCells(board, random);
        PlaceLayer(board, level, LevelLayer.Floor);
        PlaceLayer(board, level, LevelLayer.Overlay);

        return board;
    }

    public static List<LevelGoal> BuildGoals(LevelData level)
    {
        var goals = new List<LevelGoal>();
        foreach (LevelGoalData data in level.goals)
        {
            if (!Enum.TryParse(data.type, out GoalType type))
            {
                continue;
            }
            Enum.TryParse(data.color, out PieceColor color);
            goals.Add(new LevelGoal { Type = type, Color = color, TargetCount = data.count });
        }
        return goals;
    }

    private static void PlaceLayer(Board board, LevelData level, LevelLayer layer)
    {
        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                char code = level.At(layer, x, y);
                if (code == LevelData.Empty)
                {
                    continue;
                }

                ILevelElement element = LevelElements.Get(layer, code);
                if (element == null)
                {
                    throw new InvalidOperationException($"Unknown {layer} code '{code}' at ({x},{y}).");
                }
                element.Place(board, x, y);
            }
        }
    }
}