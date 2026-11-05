using UnityEngine;

public class RocketFlyViewCommand : IViewCommand
{
    private readonly int _x;
    private readonly int _y;
    private readonly RocketDirection _direction;
    private readonly TurnPlayer _player;

    public float Duration { get; }

    public RocketFlyViewCommand(RocketDirection Direction, int x, int y, TurnPlayer player)
    {
        _x = x;
        _y = y;
        _direction = Direction;
        _player = player;

        int forward = _direction == RocketDirection.Horizontal ? player.BoardWidth - 1 - x : player.BoardHeight - 1 - y;
        int backward = _direction == RocketDirection.Horizontal ? x : y;
        Duration = Mathf.Max(forward, backward) * ActivationTiming.RocketMsPerCell / 1000f;
    }

    public void Execute()
    {
        if (_direction == RocketDirection.Horizontal)
        {
            Play(_player.BoardWidth - 1, _y);
            Play(0, _y);
        }
        else
        {
            Play(_x, _player.BoardHeight - 1);
            Play(_x, 0);
        }
    }

    private void Play(int toX, int toY)
    {
        int distanceInCells = _direction == RocketDirection.Horizontal ? Mathf.Abs(toX - _x) : Mathf.Abs(toY - _y);
        if (distanceInCells <= 0) return;

        Vector3 origin = _player.CellToWorld(_x, _y);
        Vector3 target = _player.CellToWorld(toX, toY);
        Vector3 direction = (target - origin).normalized;

        RocketActivationView view = _player.SpawnRocketActivation(origin);
        view.SetDirection(direction);
        
        float duration = distanceInCells * ActivationTiming.RocketMsPerCell / 1000f;
        view.Play(target, duration, () => _player.ReleaseRocketActivation(view));
    }
}