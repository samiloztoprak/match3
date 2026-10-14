using System.Collections.Generic;

public class DropfallState : IGameState
{
    private GameLoopEnvironment _gameLoopEnvironment;
    private List<(int, int, int, int)> _collectedMovement;

    public DropfallState(GameLoopEnvironment gameLoopEnvironment)
    {
        _collectedMovement = new List<(int, int, int, int)>();
        _gameLoopEnvironment = gameLoopEnvironment;
    }

    public void Enter()
    {
        _collectedMovement = LogicLibrary.OrganizationLogic.FillEmpties(_gameLoopEnvironment.board);
    }

    public IGameState Execute()
    {
        return new WaitingForPresentationState(_gameLoopEnvironment, _collectedMovement);
    }

    public void Exit()
    {

    }
}
