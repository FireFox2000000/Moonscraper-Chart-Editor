using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SongObjectPool {
    GameObject parent;
    GameObject prefab;
    SongObjectController[] pool;

    const int POOL_EXTEND_SIZE = 50;

    public SongObjectPool(GameObject parent, GameObject prefab, int initialSize)
    {
        if (!prefab.GetComponentInChildren<SongObjectController>())
            throw new System.Exception("No SongObjectController attached to prefab");

        pool = new SongObjectController[initialSize];

        for (int i = 0; i < pool.Length; ++i)
        {
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.SetParent(parent.transform);
            pool[i] = gameObject.GetComponentInChildren<SongObjectController>();

            gameObject.SetActive(false);
        }

        this.parent = parent;
        this.prefab = prefab;
    }

    public void Reset()
    {
        foreach (SongObjectController controller in pool)
            controller.gameObject.SetActive(false);
    }

    protected void Activate(SongObject[] range)
    {
        int pos = 0;
        foreach (SongObject songObject in range)
        {
            if (songObject.controller == null)
            {
                // Find the next gameobject that is disabled/not in use
                while (pos < pool.Length && pool[pos].gameObject.activeSelf)
                {
                    ++pos;

                    if (pos >= pool.Length)
                        ExtendPool(ref pool, prefab, POOL_EXTEND_SIZE, parent);
                }

                if (pos < pool.Length && !pool[pos].gameObject.activeSelf)
                    Assign(pool[pos], songObject);
                else
                    break;
            }
        }
    }

    protected abstract void Assign(SongObjectController sCon, SongObject songObject);

    static void ExtendPool<T>(ref T[] array, GameObject prefab, int count, GameObject parent) where T : SongObjectController
    {
        if (array.Length <= 0)
            throw new System.Exception("Array not previously instanciated");

        System.Array.Resize(ref array, count + array.Length);

        for (int i = array.Length - count; i < array.Length; ++i)
        {
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.SetParent(parent.transform);
            array[i] = gameObject.GetComponentInChildren<T>();

            gameObject.SetActive(false);
        }

        Debug.Log(parent.name + " pool extended by " + count + ". Total: " + array.Length);
    }
}
