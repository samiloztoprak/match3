
public class IdleState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    
    public IdleState(GameLoopEnvironment gameLoopEnvironment)
    {
        _gameLoopEnvironment = gameLoopEnvironment;
    }

    public void Enter()
    {

    }
    
    public IGameState Execute()
    {
        ICommand command = null;
        if(_gameLoopEnvironment.pendingCommandSlot.TryTake(out command))
        {
            return new InteractState(_gameLoopEnvironment, command);
        }
        return null;
    }
    
    public void Exit()
    {

    }
}
