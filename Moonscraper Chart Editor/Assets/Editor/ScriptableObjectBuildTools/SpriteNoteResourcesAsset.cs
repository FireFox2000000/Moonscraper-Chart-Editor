using UnityEngine;
using UnityEditor;

public class SpriteNoteResourcesAsset
{
    [MenuItem("Assets/Create/Sprite Note Resources")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<SpriteNoteResources>();
    }
}
