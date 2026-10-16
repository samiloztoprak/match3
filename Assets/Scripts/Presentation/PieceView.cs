using UnityEngine;

public class PieceView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private int _pieceId;
    
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(Piece piece)
    {
        _pieceId = piece.Id;

        switch (piece.Color)
        {
            case PieceType.Blue:
                _spriteRenderer.color = Color.blue;
                break;
            case PieceType.Red:
                _spriteRenderer.color = Color.red;
                break;
            case PieceType.Yellow:
                _spriteRenderer.color = Color.yellow;
                break;
            case PieceType.Green:
                _spriteRenderer.color = Color.green;
                break;
            default:
                break;
        }
    }
}
