using System.Collections.Generic;

public class RefreshState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    List<(int, int, int, int)> _movementList;

    public RefreshState(GameLoopEnvironment gameLoopEnvironment)
    {
        _gameLoopEnvironment = gameLoopEnvironment;
    }

    public void Enter()
    {
        _movementList = LogicLibrary.OrganizationLogic.GetNewPieceToEmpty(_gameLoopEnvironment.board);
    }
    
    public IGameState Execute()
    {
        if(_movementList.Count > 0)
        { 
            return new WaitingForPresentationState(_gameLoopEnvironment, _movementList); 
        }
        return new CalculationState(_gameLoopEnvironment, null);
    }
    
    public void Exit()
    {

    }
}
