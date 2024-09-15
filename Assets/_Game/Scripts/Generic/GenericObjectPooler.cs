using UnityEngine;
using System;
using System.Collections.Generic;

public class GenericObjectPooler<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> pool = new Queue<T>();
    private List<T> activeObjects = new List<T>();
    private Action<T> onCreateObject;
    private Action<T> onGetObject;
    private Action<T> onReleaseObject;

    public GenericObjectPooler(T prefab, int initialSize, Transform parent = null,
        Action<T> onCreateObject = null, Action<T> onGetObject = null, Action<T> onReleaseObject = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.onCreateObject = onCreateObject;
        this.onGetObject = onGetObject;
        this.onReleaseObject = onReleaseObject;

        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    private T CreateObject()
    {
        T obj = UnityEngine.Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        onCreateObject?.Invoke(obj);
        return obj;
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : CreateObject();
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);
        onGetObject?.Invoke(obj);
        return obj;
    }

    public void Release(T obj)
    {
        if (activeObjects.Remove(obj))
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            onReleaseObject?.Invoke(obj);
        }
        else
        {
            Debug.LogWarning($"Attempted to release an object that is not managed by this pool: {obj.name}");
        }
    }

    public void ReleaseAll()
    {
        foreach (T obj in activeObjects.ToArray())
        {
            Release(obj);
        }
    }

    public int ActiveCount => activeObjects.Count;
    public int PooledCount => pool.Count;
}
