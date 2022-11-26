// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class SelectedHighlightDisplaySystem : SystemManagerState.System
{
    ChartEditor editor;

    GameObject[] selectedHighlightPool = new GameObject[100];
    GameObject selectedHighlightPoolParent;

    // Use this for initialization
    public SelectedHighlightDisplaySystem()
    {
        editor = ChartEditor.Instance;

        GameObject selectedHighlight = editor.assets.selectedHighlight;
        selectedHighlight.SetActive(false);

        selectedHighlightPoolParent = new GameObject("Selected Highlight Pool");
        for (int i = 0; i < selectedHighlightPool.Length; ++i)
        {
            selectedHighlightPool[i] = Object.Instantiate(selectedHighlight);
            selectedHighlightPool[i].SetActive(false);
            selectedHighlightPool[i].transform.SetParent(selectedHighlightPoolParent.transform);
        }
    }
	
	// Update is called once per frame
	public override void SystemUpdate () {
        int index, length;
        IList<SongObject> viewRange = editor.selectedObjectsManager.currentSelectedObjects;
        SongObjectHelper.GetRange(viewRange, editor.minPos, editor.maxPos, out index, out length);

        var currentTool = ChartEditor.Instance.toolManager.currentToolId;
        bool validTool = currentTool != EditorObjectToolManager.ToolID.Note && currentTool != EditorObjectToolManager.ToolID.Starpower;
        bool showHighlight = editor.currentState != ChartEditor.State.Playing && validTool;

        int pos = index;

        foreach (GameObject selectedHighlight in selectedHighlightPool)
        {
            if (showHighlight && pos < index + length && viewRange[pos].controller != null && viewRange[pos].controller.gameObject.activeSelf)
            {
                Collider col3d = viewRange[pos].controller.GetComponent<Collider>();
                Collider2D col = viewRange[pos].controller.GetComponent<Collider2D>();

                Bounds bounds;

                if (col3d)
                {
                    bounds = col3d.bounds;
                }
                else
                {
                    bounds = col.bounds;
                }

                if (bounds.size.z == 0)
                {
                    var size = bounds.size;
                    size.z = 0.1f;
                    bounds.size = size;
                }

                selectedHighlight.transform.localPosition = bounds.center;
                selectedHighlight.transform.localScale = bounds.size;

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

    public override void SystemExit()
    {
        foreach (GameObject selectedHighlight in selectedHighlightPool)
        {
            selectedHighlight.SetActive(false);
        }
    }
}
