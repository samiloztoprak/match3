using System.Collections.Generic;

public class WaitingForPresentationState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    List<(int, int, int, int)> _movementList;

    public WaitingForPresentationState(GameLoopEnvironment gameLoopEnvironment, List<(int, int, int, int)> movementList)
    {
        _gameLoopEnvironment = gameLoopEnvironment;
        _movementList = movementList;
    }

    public void Enter()
    {
        _gameLoopEnvironment.pendingPresentationSlot.Submit(_movementList);
    }
    
    public IGameState Execute()
    {
        if(_gameLoopEnvironment.pendingPresentationSlot.IsComplete())
        {
            return new IdleState(_gameLoopEnvironment); 
        }
        return null;
    }
    
    public void Exit()
    {

    }
}
