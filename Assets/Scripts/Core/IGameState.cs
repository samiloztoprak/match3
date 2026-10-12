
public interface IGameState
{
    void Enter();
    IGameState Execute();
    void Exit();
}
