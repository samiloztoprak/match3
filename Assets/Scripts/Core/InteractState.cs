
public class InteractState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    private ICommand _command;

    public InteractState(GameLoopEnvironment gameLoopEnvironment,ICommand command)
    {
        _gameLoopEnvironment = gameLoopEnvironment;
        _command = command;
    }

    public void Enter()
    {

    }
    
    public IGameState Execute()
    {
        if(_command != null)
        {
            _command.Execute();
            return new CalculationState(_gameLoopEnvironment, _command);
        }
        return null;
    }
    
    public void Exit()
    {

    }
}
