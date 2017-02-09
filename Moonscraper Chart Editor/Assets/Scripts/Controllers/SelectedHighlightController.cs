using UnityEngine;
using System.Collections;

public class SelectedHighlightController : MonoBehaviour {
    public GameObject selectedHighlight;
    ChartEditor editor;

    GameObject[] selectedHighlightPool = new GameObject[100];
    GameObject selectedHighlightPoolParent;

    // Use this for initialization
    void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        selectedHighlight.SetActive(false);

        selectedHighlightPoolParent = new GameObject("Selected Highlight Pool");
        for (int i = 0; i < selectedHighlightPool.Length; ++i)
        {
            selectedHighlightPool[i] = Instantiate(selectedHighlight);
            selectedHighlightPool[i].SetActive(false);
            selectedHighlightPool[i].transform.SetParent(selectedHighlightPoolParent.transform);
        }
    }
	
	// Update is called once per frame
	void Update () {
        SongObject[] viewRange = SongObject.GetRange(editor.currentSelectedObjects, editor.minPos, editor.maxPos);
        
        int pos = 0;
        foreach (GameObject selectedHighlight in selectedHighlightPool)
        {
            if (Globals.applicationMode != Globals.ApplicationMode.Playing && pos < viewRange.Length && viewRange[pos].controller != null && viewRange[pos].controller.gameObject.activeSelf)
            {
                selectedHighlight.transform.position = viewRange[pos].controller.transform.position;

                Collider col3d = viewRange[pos].controller.GetComponent<Collider>();
                Collider2D col = viewRange[pos].controller.GetComponent<Collider2D>();

                Vector3 scale = viewRange[pos].controller.transform.localScale;

                if (col3d)
                    scale = col3d.bounds.size;
                else
                    scale = col.bounds.size;

                if (scale.z == 0)
                    scale.z = 0.1f;
                selectedHighlight.transform.localScale = scale;

                selectedHighlight.SetActive(true);
                ++pos;
            }
            else
                selectedHighlight.SetActive(false);
        }
        /*
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
            */
    }
}
