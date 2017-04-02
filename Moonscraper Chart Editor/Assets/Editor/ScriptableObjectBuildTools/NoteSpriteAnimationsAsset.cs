using UnityEngine;
using UnityEditor;

public class NoteSpriteAnimationsAsset
{
    [MenuItem("Assets/Create/Note Sprite Animations")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<NoteSpriteAnimations>();
    }
}
