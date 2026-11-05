
public class ShakeViewCommand : IViewCommand
{
    private readonly TurnPlayer _player;
    private readonly float _amount;

    public ShakeViewCommand(TurnPlayer player, float amount)
    {
        _player = player;
        _amount = amount;
    }
    public float Duration => 0f;
    public void Execute()
    {
        if (_player.CameraShake != null)
        {
            _player.CameraShake.Add(_amount);
        }
    }
}