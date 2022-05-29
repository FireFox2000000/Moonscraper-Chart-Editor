/*
    http://wiki.unity3d.com/index.php?title=Secure_UnitySingleton&oldid=19397
	Author: Jon Kenkel (nonathaj)
	Date: 2/9/16
*/
using UnityEngine;
using System;

/// <summary>
/// Class for holding singleton component instances in Unity.
/// </summary>
/// <example>
/// [UnitySingleton(UnitySingletonAttribute.Type.LoadedFromResources, false, "test")]
/// public class MyClass : UnitySingleton&lt;MyClass&gt; { }
/// </example>
/// <example>
/// [UnitySingleton(UnitySingletonAttribute.Type.CreateOnNewGameObject)]
/// public class MyOtherClass : UnitySingleton&lt;MyOtherClass&gt; { }
/// </example>
/// <typeparam name="T">The type of the singleton</typeparam>
public abstract class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Is there an instance active of this singleton?
    /// </summary>
    public static bool InstanceExists { get { return instance != null; } }

    private static T instance = null;
    /// <summary>
    /// Returns an instance of this singleton (if it does not exist, generates one based on T's UnitySingleton Attribute settings)
    /// </summary>
    public static T Instance
    {
        get
        {
            TouchInstance();
            return instance;
        }
        set
        {
            UnitySingletonAttribute attribute = Attribute.GetCustomAttribute(typeof(T), typeof(UnitySingletonAttribute)) as UnitySingletonAttribute;
            if (attribute == null)
                Debug.LogError("Cannot find UnitySingleton attribute on " + typeof(T).Name);

            if (attribute.allowSetInstance)
                instance = value;
            else
                Debug.LogError(typeof(T).Name + " is not allowed to set instances.  Please set the allowSetInstace flag to true to enable this feature.");
        }
    }

    /// <summary>
    /// Destroy the current static instance of this singleton
    /// </summary>
    /// <param name="destroyGameObject">Should we destroy the gameobject of the instance too?</param>
    public static void DestroyInstance(bool destroyGameObject = true)
    {
        if (InstanceExists)
        {
            if (destroyGameObject)
                Destroy(instance.gameObject);
            else
                Destroy(instance);
            instance = null;
        }
    }

    /// <summary>
    /// Called when this object is created.  Children should call this base method when overriding.
    /// </summary>
    protected virtual void Awake()
    {
        if (InstanceExists && instance != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// Ensures that an instance of this singleton is generated
    /// </summary>
    public static void TouchInstance()
    {
        if (!InstanceExists)
            Generate();
    }

    /// <summary>
    /// Generates this singleton
    /// </summary>
    private static void Generate()
    {
        UnitySingletonAttribute attribute = Attribute.GetCustomAttribute(typeof(T), typeof(UnitySingletonAttribute)) as UnitySingletonAttribute;
        if (attribute == null)
        {
            Debug.LogError("Cannot find UnitySingleton attribute on " + typeof(T).Name);
            return;
        }

        for (int x = 0; x < attribute.singletonTypePriority.Length; x++)
        {
            if (TryGenerateInstance(attribute.singletonTypePriority[x], attribute.destroyOnLoad, attribute.resourcesLoadPath, x == attribute.singletonTypePriority.Length - 1))
                break;
        }
    }

    /// <summary>
    /// Attempts to generate a singleton with the given parameters
    /// </summary>
    /// <param name="type"></param>
    /// <param name="resourcesLoadPath"></param>
    /// <param name="warn"></param>
    /// <returns></returns>
    private static bool TryGenerateInstance(UnitySingletonAttribute.Type type, bool destroyOnLoad, string resourcesLoadPath, bool warn)
    {
        if (type == UnitySingletonAttribute.Type.ExistsInScene)
        {
            instance = GameObject.FindObjectOfType<T>();
            if (instance == null)
            {
                if (warn)
                    Debug.LogError("Cannot find an object with a " + typeof(T).Name + " .  Please add one to the scene.");
                return false;
            }
        }
        else if (type == UnitySingletonAttribute.Type.LoadedFromResources)
        {
            if (string.IsNullOrEmpty(resourcesLoadPath))
            {
                if (warn)
                    Debug.LogError("UnitySingletonAttribute.resourcesLoadPath is not a valid Resources location in " + typeof(T).Name);
                return false;
            }
            T pref = Resources.Load<T>(resourcesLoadPath);
            if (pref == null)
            {
                if (warn)
                    Debug.LogError("Failed to load prefab with " + typeof(T).Name + " component attached to it from folder Resources/" + resourcesLoadPath + ".  Please add a prefab with the component to that location, or update the location.");
                return false;
            }
            instance = Instantiate<T>(pref);
            if (instance == null)
            {
                if (warn)
                    Debug.LogError("Failed to create instance of prefab " + pref + " with component " + typeof(T).Name + ".  Please check your memory constraints");
                return false;
            }
        }
        else if (type == UnitySingletonAttribute.Type.CreateOnNewGameObject)
        {
            GameObject go = new GameObject(typeof(T).Name + " Singleton");
            if (go == null)
            {
                if (warn)
                    Debug.LogError("Failed to create gameobject for instance of " + typeof(T).Name + ".  Please check your memory constraints.");
                return false;
            }
            instance = go.AddComponent<T>();
            if (instance == null)
            {
                if (warn)
                    Debug.LogError("Failed to add component of " + typeof(T).Name + "to new gameobject.  Please check your memory constraints.");
                Destroy(go);
                return false;
            }
        }

        if (!destroyOnLoad)
            DontDestroyOnLoad(instance.gameObject);

        return true;
    }
}
