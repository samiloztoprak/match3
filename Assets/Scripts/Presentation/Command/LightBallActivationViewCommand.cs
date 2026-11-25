using UnityEngine;

public class LightBallActivationViewCommand : IViewCommand
{
    private readonly TurnPlayer _player;
    private readonly Vector3 _position;
    private readonly float _duration;
    private readonly LightBallActivationView _prefab;
    
    public float Duration => _duration;

    public LightBallActivationViewCommand(TurnPlayer player, Vector3 position, float duration, LightBallActivationView prefab = null)
    {
        _player = player;
        _position = position;
        _duration = duration;
        _prefab = prefab != null ? prefab : player.LightBallActivationPrefab;
    }

    public void Execute()
    {
        LightBallActivationView fx = _player.Effects.Spawn(_prefab, _position);
        fx.Play(_duration, () => _player.Effects.Release(fx));
    }
}
