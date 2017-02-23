using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisuals3DManager : NoteVisualsManager
{
    MeshFilter meshFilter;

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
}
