using UnityEngine;

public class LightBallActivationViewCommand : IViewCommand
{
    private readonly TurnPlayer _player;
    private readonly Vector3 _position;
    private readonly float _duration;

    public float Duration => _duration;

    public LightBallActivationViewCommand(TurnPlayer player, Vector3 position, float duration)
    {
        _player = player;
        _position = position;
        _duration = duration;
    }

    public void Execute()
    {
        LightBallActivationView fx = _player.Effects.Spawn(_player.LightBallActivationPrefab, _position);
        fx.Play(_duration, () => _player.Effects.Release(fx));
    }
}
