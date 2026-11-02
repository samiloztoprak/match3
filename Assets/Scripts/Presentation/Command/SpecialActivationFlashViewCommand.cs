using UnityEngine;

public class SpecialActivationFlashViewCommand : IViewCommand
{
    private readonly int _pieceId;
    private readonly int _x;
    private readonly int _y;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public SpecialActivationFlashViewCommand(int pieceId, int x, int y, TurnPlayer player, float duration)
    {
        _pieceId = pieceId;
        _x = x;
        _y = y;
        _player = player;
        _duration = duration;
    }
    
    public float Duration => _duration;

    public void Execute()
    {
        PieceView view = _player.GetView(_pieceId);
        Vector3 position = view != null ? view.transform.position : _player.CellToWorld(_x, _y);
        _player.SpawnBreakEffect(position);
    }
}
