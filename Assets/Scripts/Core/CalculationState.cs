using System.Collections.Generic;

public class CalculationState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    private ICommand _command;

    public CalculationState(GameLoopEnvironment gameLoopEnvironment, ICommand command)
    {
        _gameLoopEnvironment = gameLoopEnvironment;
        _command = command;
    }

    public void Enter()
    {

    }
    
    public IGameState Execute()
    {
        HashSet<(int, int)> matchSet = LogicLibrary.MatchLogic.FindMatches(_gameLoopEnvironment.board);
        if(matchSet.Count > 0)
        {
            foreach (var (x, y) in matchSet)
            {
                 _gameLoopEnvironment.board.ClearCell(x, y);
            }
            
            return new DropfallState(_gameLoopEnvironment);
        }
        _command.Execute();
        return new IdleState(_gameLoopEnvironment); 
    }
    
    public void Exit()
    {

    }
}
