using UnityEngine;

public class PropellerComboFlyViewCommand : IViewCommand
{
    private readonly Vector3 _origin;
    private readonly Vector3 _target;
    private readonly TurnPlayer _player;
    private readonly float _flightDuration;
    
    public float Duration => _flightDuration;
    
    public PropellerComboFlyViewCommand(Vector3 origin, Vector3 target, TurnPlayer player, float flightDuration)
    {
        _origin = origin;
        _target = target;
        _player = player;
        _flightDuration = flightDuration;
    }

    public void Execute()
    {
        AudioManager.Play(SoundId.Propeller);

        PropellerActivationView view = _player.SpawnPropellerActivation(_origin);
        view.Fly(_target, _flightDuration, _player.PropellerConfig, () => _player.ReleasePropellerActivation(view));
    }
}
