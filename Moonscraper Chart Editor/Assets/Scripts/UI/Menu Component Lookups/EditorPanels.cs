using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPanels : MonoBehaviour {

    public DisplayProperties displayProperties { get; private set; }

	// Use this for initialization
	void Start () {
        displayProperties = GetComponentInChildren<DisplayProperties>();

        Debug.Assert(displayProperties, "Unable to locate Display Properties script");
    }
}
