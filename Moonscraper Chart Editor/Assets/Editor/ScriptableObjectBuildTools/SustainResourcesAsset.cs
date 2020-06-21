using UnityEngine;
using UnityEditor;

public class SustainResourcesAsset
{
    [MenuItem("Assets/Create/Sustain Resources")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<SustainResources>();
    }
}
