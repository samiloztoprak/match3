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
        else if (piece.VisualType == PieceVisualType.Grass)
        {
            return GetGrassSprite(piece);
        }
        else if (piece.VisualType == PieceVisualType.Cage)
        {
            return GetCageSprite(piece);
        }
        return null;
    }
    
    public Sprite GetSprite(Piece piece, int healthOverride)
    {
        if (piece.VisualType == PieceVisualType.Box)
        {
            return _boxVisualConfig.GetSprite(healthOverride);
        }
        if (piece.VisualType == PieceVisualType.Lock)
        {
            return _lockVisualConfig.GetSprite(healthOverride);
        }
        if (piece.VisualType == PieceVisualType.Grass)
        {
            return _grassVisualConfig.GetSprite(healthOverride);
        }
        if (piece.VisualType == PieceVisualType.Cage)
        {
            return _cageVisualConfig.GetSprite(healthOverride);
        }
        return GetSprite(piece);
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
        if (piece is SpecialPiece)
        {
            return _specialVisualConfig.GetSprite(piece);
        }
        return null;
    }
    
    [SerializeField] 
    private GrassVisualConfig _grassVisualConfig;

    public Sprite GetGrassSprite(Piece piece)
    {
        if (piece is GrassPiece)
        {
            return _grassVisualConfig.GetSprite(piece.Health);
        }
        return null;
    }
    
    [SerializeField] 
    private CageVisualConfig _cageVisualConfig;

    public Sprite GetCageSprite(Piece piece)
    {
        if (piece is CagePiece)
        {
            return _cageVisualConfig.GetSprite(piece.Health);
        }
        return null;
    }
}
