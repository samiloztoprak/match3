using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceVisualConfig", menuName = "Match3/PieceViewConfig")]
public class PieceVisualConfig : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public PieceColor Color;
        public Sprite Sprite;
    }
    
    [SerializeField]
    private List<Entry> _entries;

    public Sprite GetSprite(Piece piece)
    {
        if(piece.VisualType == PieceVisualType.Standard)
        {
            foreach (var entry in _entries)
            {
                if (entry.Color == piece.Color)
                {
                    return entry.Sprite;
                }
            }
        }
        else if (piece.VisualType == PieceVisualType.Box)
        {
            return GetBoxSprite(piece);
        }
        return null;
    }
    
    [SerializeField] 
    private BoxVisualConfig _boxVisualConfig;

    public Sprite GetBoxSprite(Piece piece)
    {
        if (piece is BoxPiece)
        {
            return _boxVisualConfig.GetSprite(piece.Health);
        }
        return null;
    }
}
