using UnityEngine;

public sealed class CollectionPage : MonoBehaviour
{
    [SerializeField] private CollectionCatalog _catalog;
    [SerializeField] private RectTransform _content;
    [SerializeField] private CollectionSetView _setPrefab;

    private CollectionSetView[] _sets;

    internal void Prepare()
    {
        if (_sets != null) return;

        CollectionCatalog catalog = CollectionCatalogSource.Get(_catalog);
        _sets = new CollectionSetView[catalog.Sets.Count];
        for (int index = 0; index < _sets.Length; index++)
        {
            CollectionCatalog.Set set = catalog.Sets[index];
            CollectionSetView view = Instantiate(_setPrefab, _content);
            view.name = "ui_collection_set_" + set.Id;
            view.Build(set);
            _sets[index] = view;
        }
    }

    private void OnEnable()
    {
        Prepare();
        var ownedCards = MetaProgress.State.OwnedCards;
        foreach (CollectionSetView view in _sets)
            view.Refresh(ownedCards);
    }

}
