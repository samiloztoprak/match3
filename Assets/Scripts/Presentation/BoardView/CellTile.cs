using UnityEngine;

public class CellTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color LightColor;
    [SerializeField] private Color DarkColor;
    
    void OnValidate()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void SetCellLightConfig()
    {
        _spriteRenderer.color = LightColor;
        _spriteRenderer.sortingOrder = SortingOrders.LightCellTile;
    }
    
    public void SetCellDarkConfig()
    {
        _spriteRenderer.color = DarkColor;
        _spriteRenderer.sortingOrder = SortingOrders.DarkCellTile;
    }
}
