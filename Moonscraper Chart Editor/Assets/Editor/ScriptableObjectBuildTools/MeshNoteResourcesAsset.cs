using UnityEngine;
using UnityEditor;

public class MeshNoteResourcesAsset
{
    [MenuItem("Assets/Create/Mesh Note Resources")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<MeshNoteResources>();
    }
}
