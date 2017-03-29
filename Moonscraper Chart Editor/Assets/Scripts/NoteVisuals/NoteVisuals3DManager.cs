using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisuals3DManager : NoteVisualsManager
{
    MeshFilter meshFilter;
    public MeshNoteResources resources;

    // Use this for initialization
    protected override void Awake ()
    {
        base.Awake();
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    public override void UpdateVisuals () {

        base.UpdateVisuals();

        Note note = nCon.note;

        if (note != null)
        {
            // Visuals
            // Update mesh
            if (note.fret_type == Note.Fret_Type.OPEN)
                meshFilter.sharedMesh = resources.openModel.sharedMesh;
            else if (specialType == Note.Special_Type.STAR_POW)
                meshFilter.sharedMesh = resources.spModel.sharedMesh;
            else
                meshFilter.sharedMesh = resources.standardModel.sharedMesh;

            Material[] materials;

            // Determine materials
            if (note.fret_type == Note.Fret_Type.OPEN)
            {
                materials = resources.openRenderer.sharedMaterials;

                if (specialType == Note.Special_Type.STAR_POW)
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = resources.openMaterials[3];
                    else
                        materials[2] = resources.openMaterials[2];
                }
                else
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = resources.openMaterials[1];
                    else
                        materials[2] = resources.openMaterials[0];
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
                            materials = resources.spHopoRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = resources.hopoRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.strumColors[(int)note.fret_type];
                        }
                        break;
                    case (Note.Note_Type.TAP):
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = resources.spTapRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.tapColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = resources.tapRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.tapColors[(int)note.fret_type];
                        }
                        break;
                    default:    // strum
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = resources.spStrumRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = resources.strumRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.strumColors[(int)note.fret_type];
                        }
                        break;
                }
            }
            noteRenderer.sharedMaterials = materials;
        }
    }
}
