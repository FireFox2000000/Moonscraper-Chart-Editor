// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class SelectedHighlightController : MonoBehaviour {
    public GameObject selectedHighlight;
    ChartEditor editor;

    GameObject[] selectedHighlightPool = new GameObject[100];
    GameObject selectedHighlightPoolParent;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.Instance;
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
        int index, length;
        IList<SongObject> viewRange = editor.currentSelectedObjects;
        SongObjectHelper.GetRange(viewRange, editor.minPos, editor.maxPos, out index, out length);

        bool showHighlight = (editor.currentState != ChartEditor.State.Playing &&
            (Toolpane.currentTool != Toolpane.Tools.Note && Toolpane.currentTool != Toolpane.Tools.Starpower));

        int pos = index;

        foreach (GameObject selectedHighlight in selectedHighlightPool)
        {
            if (showHighlight && pos < index + length && viewRange[pos].controller != null && viewRange[pos].controller.gameObject.activeSelf)
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
