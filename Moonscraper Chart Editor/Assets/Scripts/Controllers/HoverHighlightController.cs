// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class HoverHighlightController : MonoBehaviour {
    public GameObject hoverHighlight;

    GameObject[] highlights = new GameObject[5];
    GameObject hoverHighlightParent;

    Renderer hoverHighlightRen;
    Color initColor;

	// Use this for initialization
	void Start () {
        hoverHighlightParent = new GameObject("Hover Highlights");

        hoverHighlightRen = hoverHighlight.GetComponent<Renderer>();
        initColor = hoverHighlightRen.sharedMaterial.color;

        for (int i = 0; i < highlights.Length; ++i)
        {
            highlights[i] = Instantiate(hoverHighlight);
            highlights[i].transform.SetParent(hoverHighlightParent.transform);
            highlights[i].SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        // Show a preview if the user will click on an object
        GameObject songObject = Mouse.currentSelectableUnderMouse;
        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }

        bool showHighlight = (Globals.applicationMode == Globals.ApplicationMode.Editor && !Input.GetMouseButton(0) && songObject != null
            && ((Toolpane.currentTool == Toolpane.Tools.Cursor || Toolpane.currentTool == Toolpane.Tools.Eraser) ||
            (Input.GetMouseButton(1) && (Toolpane.currentTool != Toolpane.Tools.Cursor || Toolpane.currentTool != Toolpane.Tools.Eraser || Toolpane.currentTool != Toolpane.Tools.GroupSelect))));

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
            List<GameObject> songObjects = new List<GameObject>();

            if (ShortcutInput.GetInput(Shortcut.ChordSelect))
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

    void OnApplicationQuit()
    {
        hoverHighlightRen.sharedMaterial.color = initColor;
    }
}
