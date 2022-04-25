/*
    http://wiki.unity3d.com/index.php?title=Secure_UnitySingleton&oldid=19397
	Author: Jon Kenkel (nonathaj)
	Date: 2/9/16
*/
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class UnitySingletonAttribute : Attribute
{
    /// <summary>
    /// What kind of singleton is this and how should it be generated?
    /// </summary>
    public enum Type
    {
        ExistsInScene,                  ///already exists in the scene, just look for it
		LoadedFromResources,            ///load from the Resources folder, at the given path
		CreateOnNewGameObject,          ///Create a new gameobject and create this singleton on it
	}

    public readonly Type[] singletonTypePriority;
    public readonly bool destroyOnLoad;
    public readonly string resourcesLoadPath;
    public readonly bool allowSetInstance;

    public UnitySingletonAttribute(Type singletonCreateType, bool destroyInstanceOnLevelLoad = true, string resourcesPath = "", bool allowSet = false)
    {
        singletonTypePriority = new Type[] { singletonCreateType };
        destroyOnLoad = destroyInstanceOnLevelLoad;
        resourcesLoadPath = resourcesPath;
        allowSetInstance = allowSet;
    }

    public UnitySingletonAttribute(Type[] singletonCreateTypePriority, bool destroyInstanceOnLevelLoad = true, string resourcesPath = "", bool allowSet = false)
    {
        singletonTypePriority = singletonCreateTypePriority;
        destroyOnLoad = destroyInstanceOnLevelLoad;
        resourcesLoadPath = resourcesPath;
        allowSetInstance = allowSet;
    }
}