using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Settings : MonoBehaviour {
    [HideInInspector]
    public string productName;

	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        productName = UnityEditor.PlayerSettings.productName;
#endif
    }
}
