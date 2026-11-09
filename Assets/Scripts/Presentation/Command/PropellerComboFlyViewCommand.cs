using UnityEngine;

public class PropellerComboFlyViewCommand : IViewCommand
{
    private readonly Sprite _sprite;
    private readonly Vector3 _origin;
    private readonly Vector3 _target;
    private readonly TurnPlayer _player;
    private readonly float _flightDuration;
    
    public float Duration => _flightDuration;
    
    public PropellerComboFlyViewCommand(Sprite sprite, Vector3 origin, Vector3 target, TurnPlayer player, float flightDuration)
    {
        _sprite = sprite;
        _origin = origin;
        _target = target;
        _player = player;
        _flightDuration = flightDuration;
    }

    public void Execute()
    {
        PropellerActivationView view = _player.SpawnPropellerActivation(_origin);
        view.SetSprite(_sprite);
        view.Fly(_target, _flightDuration, _player.PropellerConfig, () => _player.ReleasePropellerActivation(view));
    }
}
