using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CollectionSetView : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private RectTransform _cardsRoot;
    [SerializeField] private CollectionCardView _cardPrefab;

    private CollectionCatalog.Set _set;
    private CollectionCardView[] _cards;
    private int _shownCount = -1;
    public void Build(CollectionCatalog.Set set)
    {
        _set = set;
        _cards = new CollectionCardView[set.Cards.Length];
        for (int index = 0; index < _cards.Length; index++)
        {
            _cards[index] = Instantiate(_cardPrefab, _cardsRoot);
            _cards[index].name = "ui_collection_card_" + set.Cards[index].Id;
        }
    }

    public void Refresh(List<string> ownedCards)
    {
        int found = 0;
        for (int index = 0; index < _cards.Length; index++)
        {
            CollectionCatalog.Card card = _set.Cards[index];
            bool owned = ownedCards.Contains(card.Id);
            if (owned) found++;
            _cards[index].Show(card, owned);
        }

        if (_shownCount == found) return;

        _shownCount = found;
        _title.text = $"{_set.Title}  {found}/{_cards.Length}";
    }
}
