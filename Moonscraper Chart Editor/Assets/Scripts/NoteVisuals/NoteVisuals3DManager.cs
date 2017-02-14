using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisuals3DManager : MonoBehaviour {
    public NoteController nCon;
    protected Renderer noteRenderer;
    MeshFilter meshFilter;
    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

    // Use this for initialization
    void Start () {
        noteRenderer = GetComponent<Renderer>();
        meshFilter = GetComponent<MeshFilter>();
    }
	
	// Update is called once per frame
	void Update () {
        Note note = nCon.note;

        // Note Type
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            noteType = note.type;
        }
        else
        {
            // Do this simply because the HOPO glow by itself looks pretty cool
            noteType = Note.Note_Type.HOPO;
        }

        // Star power?
        specialType = Note.Special_Type.NONE;
        foreach (Starpower sp in note.chart.starPower)
        {
            if (sp.position == note.position || (sp.position <= note.position && sp.position + sp.length > note.position))
            {
                specialType = Note.Special_Type.STAR_POW;
            }
            else if (sp.position > note.position)
                break;
        }

        // Update note visuals
        noteRenderer.sortingOrder = -Mathf.Abs((int)note.position);

        // Visuals
        // Update mesh
        if (note.fret_type == Note.Fret_Type.OPEN)
            meshFilter.sharedMesh = PrefabGlobals.openModel.sharedMesh;
        else if (specialType == Note.Special_Type.STAR_POW)
            meshFilter.sharedMesh = PrefabGlobals.spModel.sharedMesh;
        else
            meshFilter.sharedMesh = PrefabGlobals.standardModel.sharedMesh;

        Material[] materials;

        // Determine materials
        if (note.fret_type == Note.Fret_Type.OPEN)
        {
            materials = PrefabGlobals.openRenderer.sharedMaterials;

            if (specialType == Note.Special_Type.STAR_POW)
            {
                if (noteType == Note.Note_Type.HOPO)
                    materials[2] = PrefabGlobals.openMaterials[3];
                else
                    materials[2] = PrefabGlobals.openMaterials[2];
            }
            else
            {
                if (noteType == Note.Note_Type.HOPO)
                    materials[2] = PrefabGlobals.openMaterials[1];
                else
                    materials[2] = PrefabGlobals.openMaterials[0];
            }
        }
        else
        {
            const int standardColMatPos = 1;
            const int spColMatPos = 3;

            switch (noteType)
            {
                case (Note.Note_Type.HOPO):
                    if (specialType == Note.Special_Type.STAR_POW)
                    {
                        materials = PrefabGlobals.spHopoRenderer.sharedMaterials;
                        materials[spColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                    }
                    else
                    {
                        materials = PrefabGlobals.hopoRenderer.sharedMaterials;
                        materials[standardColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                    }
                    break;
                case (Note.Note_Type.TAP):
                    if (specialType == Note.Special_Type.STAR_POW)
                    {
                        materials = PrefabGlobals.spTapRenderer.sharedMaterials;
                        materials[spColMatPos] = PrefabGlobals.tapColors[(int)note.fret_type];
                    }
                    else
                    {
                        materials = PrefabGlobals.tapRenderer.sharedMaterials;
                        materials[standardColMatPos] = PrefabGlobals.tapColors[(int)note.fret_type];
                    }
                    break;
                default:    // strum
                    if (specialType == Note.Special_Type.STAR_POW)
                    {
                        materials = PrefabGlobals.spStrumRenderer.sharedMaterials;
                        materials[spColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                    }
                    else
                    {
                        materials = PrefabGlobals.strumRenderer.sharedMaterials;
                        materials[standardColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                    }
                    break;
            }
        }
        noteRenderer.sharedMaterials = materials;
    }
}
