using UnityEngine;

public class RocketFlyViewCommand : IViewCommand
{
    private readonly int _x;
    private readonly int _y;
    private readonly RocketDirection _direction;
    private readonly TurnPlayer _player;
    private readonly float _charge;

    public float Duration { get; }

    public RocketFlyViewCommand(RocketDirection direction, int x, int y, TurnPlayer player, float chargeSeconds)
    {
        _x = x;
        _y = y;
        _direction = direction;
        _player = player;
        _charge = chargeSeconds;

        int forward = _direction == RocketDirection.Horizontal ? player.BoardWidth - 1 - x : player.BoardHeight - 1 - y;
        int backward = _direction == RocketDirection.Horizontal ? x : y;
        Duration = _charge + Mathf.Max(forward, backward) * ActivationTiming.RocketMsPerCell / 1000f;
    }

    public void Execute()
    {
        AudioManager.Play(SoundId.Rocket);

        bool horizontal = _direction == RocketDirection.Horizontal;
        int distA = horizontal ? _player.BoardWidth - 1 - _x : _player.BoardHeight - 1 - _y;
        int distB = horizontal ? _x : _y;
        Vector3 targetA = horizontal ? _player.CellToWorld(_player.BoardWidth - 1, _y) : _player.CellToWorld(_x, _player.BoardHeight - 1);
        Vector3 targetB = horizontal ? _player.CellToWorld(0, _y) : _player.CellToWorld(_x, 0);
        float msToSec = ActivationTiming.RocketMsPerCell / 1000f;

        RocketActivationView view = _player.SpawnRocketActivation(_player.CellToWorld(_x, _y));
        view.Play(horizontal ? Vector3.right : Vector3.up, _charge, targetA, distA * msToSec,
            targetB, distB * msToSec, () => _player.ReleaseRocketActivation(view));
    }
}