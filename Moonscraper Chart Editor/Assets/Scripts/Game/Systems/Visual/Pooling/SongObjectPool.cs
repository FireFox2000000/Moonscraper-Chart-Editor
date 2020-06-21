// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

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

    public void SetAllDirty()
    {
        for (int i = 0; i < pool.Length; ++i)
        {
            pool[i].SetDirty();
        }
    }

    public void Activate<T>(IList<T> range, int index, int length) where T : SongObject
    {
        int pos = 0;
        for (int i = index; i < index + length; ++i)
        {
            if (range[i].controller == null)        // Check if the object is already attached to something, else we can skip it
            {
                // Find the next gameobject that is disabled/not in use
                while (pos < pool.Length && pool[pos].gameObject.activeSelf)
                {
                    ++pos;

                    if (pos >= pool.Length)
                        ExtendPool(ref pool, prefab, POOL_EXTEND_SIZE, parent);
                }

                if (pos < pool.Length && !pool[pos].gameObject.activeSelf)
                    Assign(pool[pos], range[i]);
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
