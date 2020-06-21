using UnityEngine;
using UnityEditor;

public class InputConfigAsset
{
    [MenuItem("Assets/Create/Input Config Asset")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<InputConfig>();
    }
}
