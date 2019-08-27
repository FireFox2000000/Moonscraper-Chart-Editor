using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T>
{
    static T instance;
    protected virtual bool WantDontDestroyOnLoad { get { return true; } }

    public static T Instance
    {
        get
        {
            if (instance)
                return instance;

            instance = FindObjectOfType<T>();
            if (instance)
                return instance;

            instance = InitInstance();
            return instance;
        }
    }

    static T InitInstance()
    {
        GameObject go = new GameObject();
        instance = go.AddComponent<T>();

        if (instance.WantDontDestroyOnLoad)
            DontDestroyOnLoad(go);

        go.name = "[Singleton] " + typeof(T).ToString();
        return instance;
    }

    private void OnDestroy()
    {
        if (!WantDontDestroyOnLoad)
        {
            instance = null;
        }
    }
}
