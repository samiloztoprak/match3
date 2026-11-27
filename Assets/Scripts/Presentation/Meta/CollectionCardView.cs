using UnityEngine;
using UnityEngine.UI;

public sealed class CollectionCardView : MonoBehaviour
{
    [SerializeField] private Image _art;
    [SerializeField] private Sprite _lockedArt;

    public void Show(CollectionCatalog.Card card, bool owned)
    {
        _art.sprite = owned ? card.Art : _lockedArt;
        _art.enabled = _art.sprite != null;
    }
}
