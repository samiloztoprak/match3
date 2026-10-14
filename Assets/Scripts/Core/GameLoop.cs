
public class GameLoop
{
    private GameLoopEnvironment _gameLoopEnvironment;
    private IGameState _currentState;

    public GameLoop()
    {
        _gameLoopEnvironment = new GameLoopEnvironment();
        _gameLoopEnvironment.board = new Board(5,5);
        _gameLoopEnvironment.pendingCommandSlot = new PendingCommandSlot();
        _gameLoopEnvironment.pendingPresentationSlot = new PendingPresentationSlot();
        _currentState = new IdleState(_gameLoopEnvironment);
        _currentState.Enter();
    }

    public void Tick(float deltaTime)
    {
        if(_currentState != null)
        {
            IGameState _nextState = _currentState.Execute();
            if(_nextState != null)
            {
                _currentState.Exit();
                _currentState = _nextState;
                _currentState.Enter();
            }
        }
    }
}
