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
        else if (piece.VisualType == PieceVisualType.Lock)
        {
            return GetLockSprite(piece);
        }
        else if (piece.VisualType == PieceVisualType.Special)
        {
            return GetSpecialSprite(piece);
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
    
    [SerializeField] 
    private LockVisualConfig _lockVisualConfig;

    public Sprite GetLockSprite(Piece piece)
    {
        if (piece is LockPiece)
        {
            return _lockVisualConfig.GetSprite(piece.Health);
        }
        return null;
    }
    
    [SerializeField] 
    private SpecialVisualConfig _specialVisualConfig;

    public Sprite GetSpecialSprite(Piece piece)
    {
        Debug.Log("piece");
        if (piece is SpecialPiece)
        {
            Debug.Log("piece22222222");

            return _specialVisualConfig.GetSprite(piece);
        }
        return null;
    }
}
