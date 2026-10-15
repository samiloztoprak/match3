
using System.Collections.Generic;
using LogicLibrary;

public static class TurnResolver
{
    private static readonly int MaxIteration = 100;
    public static List<TurnStepData> Resolve(Board board, ICommand command)
    {
        var result = new List<TurnStepData>();
        
        if(command != null)
            command.Execute();
        
        int iteration = 0;
        while (true)
        {
            HashSet<(int, int)> matchSet = MatchLogic.FindMatches(board);
            if (matchSet.Count == 0)
            {
                if(iteration == 0)
                {
                    //Restore execution.
                    if (command != null)
                        command.Execute();
                }
                break;
            }

            ClearStepData clearStepData = new ClearStepData();
            foreach (var match in matchSet)
            {
                board.ClearCell(match.Item1, match.Item2);
                clearStepData.Cells.Add((match.Item1, match.Item2));
            }
            result.Add(clearStepData);

            List<(int, int, int, int)> fillPieces = OrganizationLogic.FillEmpties(board);
            FallStepData fallStepData = new FallStepData();
            foreach (var piece in fillPieces)
            {
                fallStepData.Moves.Add(piece);
            }
            result.Add(fallStepData);
            
            List<(int, int, int, int)> newPieces = OrganizationLogic.GetNewPieceToEmpty(board);
            RefillStepData refillStepData = new RefillStepData();
            foreach (var piece in newPieces)
            {
                refillStepData.Moves.Add(piece);
            }
            result.Add(refillStepData);
            
            iteration++;
            if(iteration > MaxIteration)
                break;
        }
        return  result;
    }
}
