using UnityEngine;
using UnityEditor;

public class CurrentSkinAsset
{
    [MenuItem("Assets/Create/Current Skin")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<Skin>();
    }
}
