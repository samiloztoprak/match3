using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorPreview
{
    private readonly PieceVisualConfig _visuals = AutoRef.Asset<PieceVisualConfig>();
    private readonly Dictionary<ILevelElement, Piece> _samples = new Dictionary<ILevelElement, Piece>();

    public Texture GetTexture(ILevelElement element)
    {
        Piece piece = GetSample(element);
        Sprite sprite = piece != null && _visuals != null ? _visuals.GetSprite(piece) : null;
        return sprite != null ? AssetPreview.GetAssetPreview(sprite) : null;
    }

    public Vector2Int GetSize(ILevelElement element)
    {
        if (!(GetSample(element) is OverlayPiece overlay)) return Vector2Int.one;

        int maxX = 0, maxY = 0;
        foreach (var (x, y) in overlay.CoveredCells)
        {
            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);
        }
        return new Vector2Int(maxX + 1, maxY + 1);
    }

    private Piece GetSample(ILevelElement element)
    {
        if (_samples.TryGetValue(element, out Piece piece)) return piece;

        var board = new Board(4, 4);
        element.Place(board, 0, 0);
        switch (element.Layer)
        {
            case LevelLayer.Floor:
            {
                piece = board.GetFloor(0, 0);
                break;
            }
            case LevelLayer.Overlay:
            {
                piece = board.OverlayPieces.Count > 0 ? board.OverlayPieces[0] : null;
                break;
            }
            default:
            {
                piece = board.GetCell(0, 0);
                break;
            }
        }
        _samples[element] = piece;
        return piece;
    }
}