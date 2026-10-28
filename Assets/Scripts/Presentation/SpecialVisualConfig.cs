using UnityEngine;

[CreateAssetMenu(fileName = "SpecialVisualConfig", menuName = "Match3/SpecialViewConfig")]
public class SpecialVisualConfig : ScriptableObject
{
    [SerializeField] private Sprite _tntSprite;
    [SerializeField] private Sprite _propellerSprite;
    [SerializeField] private Sprite _lightBallSprite;
    [SerializeField] private Sprite _rocketHorizontalSprite;
    [SerializeField] private Sprite _rocketVerticalSprite;

    public Sprite GetSprite(Piece piece)
    {
        if (piece is RocketPiece rocketPiece)
        {
            return rocketPiece.Direction == RocketDirection.Horizontal
                ? _rocketHorizontalSprite
                : _rocketVerticalSprite;
        }

        if (piece is SpecialPiece specialPiece)
        {
            switch (specialPiece.Kind)
            {
                case PieceKind.Tnt: return _tntSprite;
                case PieceKind.Propeller: return _propellerSprite;
                case PieceKind.LightBall: return _lightBallSprite;
            }
        }
        
        return null;
    }
}
