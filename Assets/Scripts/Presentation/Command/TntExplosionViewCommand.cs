
public class TntExplosionViewCommand : IViewCommand
{
    private readonly int _x, _y;
    private readonly TurnPlayer _player;
    private readonly float _expand;

    public TntExplosionViewCommand(int x, int y, TurnPlayer player, float expand)
    {
        _x = x; _y = y; _player = player; _expand = expand;
    }

    public float Duration => _expand;
    public void Execute()
    {
        ExplosionView fx = _player.Effects.Spawn(_player.TntExplosionPrefab, _player.CellToWorld(_x, _y));
        fx.Play(_expand, 3f * _player.CellSize, () => _player.Effects.Release(fx));
    }
}