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

        bool showHighlight = (Globals.applicationMode != Globals.ApplicationMode.Playing &&
                (Toolpane.currentTool == Toolpane.Tools.Cursor || Toolpane.currentTool == Toolpane.Tools.Eraser || Toolpane.currentTool == Toolpane.Tools.GroupSelect));

        int pos = 0;

        foreach (GameObject selectedHighlight in selectedHighlightPool)
        {
            if (showHighlight && pos < viewRange.Length && viewRange[pos].controller != null && viewRange[pos].controller.gameObject.activeSelf)
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
            {
                if (!selectedHighlight.activeSelf)
                    break;

                selectedHighlight.SetActive(false);
            }
        }
    }
}
