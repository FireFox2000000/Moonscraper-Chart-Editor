// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

public class HoverHighlightDisplaySystem : SystemManagerState.System
{
    GameObject[] highlights = new GameObject[5];
    GameObject hoverHighlightParent;

    Renderer hoverHighlightRen;
    Color initColor;

    List<GameObject> songObjects = new List<GameObject>();

    // Use this for initialization
    public HoverHighlightDisplaySystem(ChartEditorAssets assets) {
        hoverHighlightParent = new GameObject("Hover Highlights");

        GameObject hoverHighlight = assets.hoverHighlight;
        hoverHighlightRen = hoverHighlight.GetComponent<Renderer>();
        initColor = hoverHighlightRen.sharedMaterial.color;

        for (int i = 0; i < highlights.Length; ++i)
        {
            highlights[i] = Object.Instantiate(hoverHighlight);
            highlights[i].transform.SetParent(hoverHighlightParent.transform);
            highlights[i].SetActive(false);
        }
	}
	
	// Update is called once per frame
	public override void SystemUpdate ()
    {
        ChartEditor editor = ChartEditor.Instance;

        // Show a preview if the user will click on an object
        GameObject songObject = editor.services.mouseMonitorSystem.currentSelectableUnderMouse;
        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }

        var currentTool = editor.toolManager.currentToolId;
        bool validTool = currentTool == EditorObjectToolManager.ToolID.Cursor || currentTool == EditorObjectToolManager.ToolID.Eraser;
        bool previewDelete = Input.GetMouseButton(1) && (currentTool != EditorObjectToolManager.ToolID.Cursor || currentTool != EditorObjectToolManager.ToolID.Eraser);
        bool showHighlight = !Input.GetMouseButton(0) && songObject != null && (validTool || previewDelete);

        if (!showHighlight)
            return;

        // Change the shared material of the highlight
        if (Input.GetMouseButton(1))
        {
            if (songObject && songObject.GetComponent<SustainController>())
                return;
            else
                hoverHighlightRen.sharedMaterial.color = new Color(Color.red.r, Color.red.g, Color.red.b, initColor.a);
        }
        else
            hoverHighlightRen.sharedMaterial.color = initColor;

        if (showHighlight)
        {
            songObjects.Clear();

            if (MSChartEditorInput.GetInput(MSChartEditorInputActions.ChordSelect))
            {
                // Check if we're over a note
                NoteController nCon = songObject.GetComponent<NoteController>();
                if (nCon)
                {
                    foreach (Note note in nCon.note.chord)
                        songObjects.Add(note.controller.gameObject);
                }
                else
                {
                    SustainController sCon = songObject.GetComponent<SustainController>();
                    if (sCon)
                    {
                        foreach (Note note in sCon.nCon.note.chord)
                            songObjects.Add(note.controller.sustain.gameObject);
                    }
                }
            }
            else
            {
                songObjects.Add(songObject);
            }

            // Activate, position and scale highlights
            for (int i = 0; i < songObjects.Count; ++i)
            {
                if (i < highlights.Length)
                {
                    highlights[i].SetActive(true);
                    highlights[i].transform.position = songObjects[i].transform.position;

                    Vector3 scale = songObjects[i].transform.localScale;
                    Collider col3d = songObjects[i].GetComponent<Collider>();
                    Collider2D col = songObjects[i].GetComponent<Collider2D>();

                    if (col3d)
                        scale = col3d.bounds.size;
                    else
                        scale = col.bounds.size;

                    if (scale.z == 0)
                        scale.z = 0.1f;
                    highlights[i].transform.localScale = scale;
                }
            }
        }
    }

    public override void SystemExit()
    {
        hoverHighlightRen.sharedMaterial.color = initColor;

        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }
    }
}
