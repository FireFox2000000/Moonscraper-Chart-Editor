using UnityEngine;
using System.Collections;

public class SelectedHighlightController : MonoBehaviour {
    public GameObject selectedHighlight;
    ChartEditor editor;
	// Use this for initialization
	void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        selectedHighlight.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        // Show a highlight over the current selected object
        SongObject currentSelectedObject = editor.currentSelectedObject;
        if (Globals.applicationMode != Globals.ApplicationMode.Playing && currentSelectedObject != null && currentSelectedObject.controller != null && currentSelectedObject.controller.gameObject != null && currentSelectedObject.controller.gameObject.activeSelf)
        {
            Collider col3d = editor.currentSelectedObject.controller.GetComponent<Collider>();
            Collider2D col = currentSelectedObject.controller.GetComponent<Collider2D>();
            if (col3d || col)
            {   
                selectedHighlight.transform.position = currentSelectedObject.controller.transform.position;

                Vector3 scale = currentSelectedObject.controller.transform.localScale;

                if (col3d)
                    scale = col3d.bounds.size;
                else
                    scale = col.bounds.size;

                if (scale.z == 0)
                    scale.z = 0.1f;
                selectedHighlight.transform.localScale = scale;

                selectedHighlight.SetActive(true);
            }
            else
                selectedHighlight.SetActive(false);
        }
        else
            selectedHighlight.SetActive(false);
    }
}
