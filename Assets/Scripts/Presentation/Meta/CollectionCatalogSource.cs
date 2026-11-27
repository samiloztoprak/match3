using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

internal static class CollectionCatalogSource
{
    private const string Address = "collection/catalog";
    private const float LoadTimeoutSeconds = 8f;
    private static AsyncOperationHandle<CollectionCatalog> _handle;
    private static CollectionCatalog _catalog;

    public static CollectionCatalog Get(CollectionCatalog fallback) => _catalog != null ? _catalog : fallback;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        if (_handle.IsValid()) Addressables.Release(_handle);
        _handle = default;
        _catalog = null;
    }

    public static IEnumerator Load()
    {
        if (_handle.IsValid()) yield break;

        _handle = Addressables.LoadAssetAsync<CollectionCatalog>(Address);
        float deadline = Time.realtimeSinceStartup + LoadTimeoutSeconds;
        while (!_handle.IsDone && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (_handle.IsDone && _handle.Status == AsyncOperationStatus.Succeeded && IsUsable(_handle.Result))
        {
            _catalog = _handle.Result;
            Debug.Log($"[Collection] Loaded Addressables catalog: {_catalog.name}");
            yield break;
        }

        Addressables.Release(_handle);
        _handle = default;
        Debug.LogWarning("[Collection] Addressables catalog unavailable. Using the bundled catalog.");
    }

    private static bool IsUsable(CollectionCatalog catalog)
    {
        if (catalog == null || catalog.Sets == null) return false;

        int numberOfCards = 0;
        foreach (CollectionCatalog.Set set in catalog.Sets)
        {
            if (set?.Cards == null || set.Cards.Length == 0) return false;
            foreach (CollectionCatalog.Card card in set.Cards)
            {
                if (card == null || string.IsNullOrEmpty(card.Id) || card.Art == null) return false;
                numberOfCards++;
            }
        }

        return numberOfCards >= CollectionLogic.CardsPerPack;
    }
}
