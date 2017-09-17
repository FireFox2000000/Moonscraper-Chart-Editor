// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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
        if (!meshFilter)
            Awake();

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
                    if (noteType == Note.Note_Type.Hopo && !Globals.drumMode)
                        materials[2] = resources.openMaterials[3];
                    else
                        materials[2] = resources.openMaterials[2];
                }
                else
                {
                    if (noteType == Note.Note_Type.Hopo && !Globals.drumMode)
                        materials[2] = resources.openMaterials[1];
                    else
                        materials[2] = resources.openMaterials[0];
                }
            }
            else
            {
                const int standardColMatPos = 1;
                const int spColMatPos = 3;

                int fretNumber = (int)note.fret_type;
                if (Globals.drumMode)
                {
                    fretNumber += 1;
                    if (fretNumber > 4)
                        fretNumber = 0;
                }

                switch (noteType)
                {
                    case (Note.Note_Type.Hopo):
                        if (Globals.drumMode)
                            goto default;

                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = resources.spHopoRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.strumColors[fretNumber];
                        }
                        else
                        {
                            materials = resources.hopoRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.strumColors[fretNumber];
                        }
                        break;
                    case (Note.Note_Type.Tap):
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = resources.spTapRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.tapColors[fretNumber];
                        }
                        else
                        {
                            materials = resources.tapRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.tapColors[fretNumber];
                        }
                        break;
                    default:    // strum
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = resources.spStrumRenderer.sharedMaterials;
                            materials[spColMatPos] = resources.strumColors[fretNumber];
                        }
                        else
                        {
                            materials = resources.strumRenderer.sharedMaterials;
                            materials[standardColMatPos] = resources.strumColors[fretNumber];
                        }
                        break;
                }
            }
            noteRenderer.sharedMaterials = materials;
        }
    }
}
