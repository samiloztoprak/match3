using System.Collections.Generic;
using UnityEngine;

public class AreaDamageViewCommand : IViewCommand
{
    private readonly List<(int x, int y)> _cells;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public AreaDamageViewCommand(List<(int x, int y)> cells, TurnPlayer player, float duration)
    {
        _cells = cells;
        _player = player;
        _duration = duration;
    }

    public float Duration => _duration;
    
    public void Execute()
    {
        foreach (var (x, y) in _cells)
            _player.SpawnBreakEffect(_player.CellToWorld(x, y));
    }
}
