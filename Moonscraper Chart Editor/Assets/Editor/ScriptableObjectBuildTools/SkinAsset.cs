using UnityEngine;
using UnityEditor;

public class SkinAsset
{
    [MenuItem("Assets/Create/Skin")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<Skin>();
    }
}
