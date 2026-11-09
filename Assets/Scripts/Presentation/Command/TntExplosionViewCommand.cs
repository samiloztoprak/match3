
public class TntExplosionViewCommand : IViewCommand
{
    private readonly int _x, _y;
    private readonly TurnPlayer _player;
    private readonly float _expand;
    private readonly float _radius;

    public TntExplosionViewCommand(int x, int y, TurnPlayer player, float expand, float radius = 1f)
    {
        _x = x; _y = y; _player = player; _expand = expand; _radius = radius;
    }

    public float Duration => _expand;
    public void Execute()
    {
        ExplosionView fx = _player.Effects.Spawn(_player.TntExplosionPrefab, _player.CellToWorld(_x, _y));
        fx.Play(_expand, (2f * _radius + 1f) * _player.CellSize, () => _player.Effects.Release(fx));
    }
}