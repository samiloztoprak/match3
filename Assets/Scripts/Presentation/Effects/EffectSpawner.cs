using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

public class EffectSpawner : MonoBehaviour
{
    private readonly Dictionary<Component, object> _poolByPrefab = new Dictionary<Component, object>();
    private readonly Dictionary<Component, object> _poolByInstance = new Dictionary<Component, object>();

    public T Spawn<T>(T prefab, Vector3 position) where T : Component
    {
        ObjectPool<T> pool = GetPool(prefab);
        T instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        _poolByInstance[instance] = pool;
        return instance;
    }
    
    public void Release<T>(T instance) where T : Component
    {
        if (_poolByInstance.TryGetValue(instance, out object pool))
        {
            ((ObjectPool<T>)pool).Release(instance);
        }
    }
    
    private ObjectPool<T> GetPool<T>(T prefab) where T : Component
    {
        if (_poolByPrefab.TryGetValue(prefab, out object existing))
        {
            return (ObjectPool<T>)existing;
        }

        var pool = new ObjectPool<T>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: e => e.gameObject.SetActive(true),
            actionOnRelease: e =>
            {
                e.transform.DOKill();
                e.gameObject.SetActive(false);
            },
            actionOnDestroy: e => Destroy(e.gameObject),
            collectionCheck: true,
            defaultCapacity: 4,
            maxSize: 32);

        _poolByPrefab[prefab] = pool;
        return pool;
    }
}
