using UnityEngine;

public class SpawnSpecialViewCommand : IViewCommand
{
    private readonly Piece _piece;
    private readonly Vector3 _position;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public SpawnSpecialViewCommand(Piece piece, Vector3 position, TurnPlayer player, float duration)
    {
        _piece = piece;
        _position = position;
        _player = player;
        _duration = duration;
    }
    
    public float Duration => _duration;
    
    public void Execute()
    {
        _player.SpawnPieceView(_piece, _position);
    }
}
