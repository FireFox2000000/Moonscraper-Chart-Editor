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
            if (note.IsOpenNote())// fret_type == Note.Fret_Type.OPEN)
                meshFilter.sharedMesh = resources.openModel.sharedMesh;
            else if (specialType == Note.SpecialType.STAR_POW)
                meshFilter.sharedMesh = resources.spModel.sharedMesh;
            else
                meshFilter.sharedMesh = resources.standardModel.sharedMesh;

            Material[] materials;

            // Determine materials
            if (note.IsOpenNote())
            {
                materials = resources.openRenderer.sharedMaterials;

                if (specialType == Note.SpecialType.STAR_POW)
                {
                    if (noteType == Note.NoteType.Hopo && !Globals.drumMode)
                        materials[2] = resources.openMaterials[3];
                    else
                        materials[2] = resources.openMaterials[2];
                }
                else
                {
                    if (noteType == Note.NoteType.Hopo && !Globals.drumMode)
                        materials[2] = resources.openMaterials[1];
                    else
                        materials[2] = resources.openMaterials[0];
                }
            }
            else if (Globals.ghLiveMode)
            {
                materials = GetGHLiveNoteColours(note);
            }
            else
            {
                materials = GetStandardNoteColours(note);
            }

            noteRenderer.sharedMaterials = materials;
        }
    }

    static Material GetValidMaterial(Material[] matArray, int fretNumber)
    {
        if (fretNumber >= 0 && fretNumber < matArray.Length)
            return matArray[fretNumber];
        else
            return null;
    }

    Material[] GetStandardNoteColours(Note note)
    {
        int fretNumber = (int)note.guitarFret;
        if (Globals.drumMode)
        {
            fretNumber += 1;
            if (fretNumber > 4)
                fretNumber = 0;
        }

        Material[] strumColorMats = resources.strumColors;
        Material[] tapColorMats = resources.tapColors;

        return GetMatFromNoteType(fretNumber, strumColorMats, tapColorMats);
    }

    Material[] GetGHLiveNoteColours(Note note)
    {
        int fretNumber;

        switch (note.ghliveGuitarFret)
        {
            case (Note.GHLiveGuitarFret.WHITE_1):
            case (Note.GHLiveGuitarFret.WHITE_2):
            case (Note.GHLiveGuitarFret.WHITE_3):
                fretNumber = 1;
                break;

            case (Note.GHLiveGuitarFret.BLACK_1):
            case (Note.GHLiveGuitarFret.BLACK_2):
            case (Note.GHLiveGuitarFret.BLACK_3):
            default:
                fretNumber = 0;
                break;
        }

        Material[] strumColorMats = resources.ghlStrumColors;
        Material[] tapColorMats = resources.ghlTapColors;

        return GetMatFromNoteType(fretNumber, strumColorMats, tapColorMats);
    }

    Material[] GetMatFromNoteType(int materialToSelectArrayPos, Material[] strumColorMats, Material[] tapColorMats)
    {
        Material[] materials;
        const int STANDARD_COLOUR_MAT_POS = 1;
        const int SP_COLOR_MAT_POS = 3;

        int fretNumber = materialToSelectArrayPos;

        switch (noteType)
        {
            case (Note.NoteType.Hopo):
                if (Globals.drumMode)
                    goto default;

                if (specialType == Note.SpecialType.STAR_POW)
                {
                    materials = resources.spHopoRenderer.sharedMaterials;
                    materials[SP_COLOR_MAT_POS] = GetValidMaterial(strumColorMats, fretNumber);
                }
                else
                {
                    materials = resources.hopoRenderer.sharedMaterials;
                    materials[STANDARD_COLOUR_MAT_POS] = GetValidMaterial(strumColorMats, fretNumber);
                }
                break;
            case (Note.NoteType.Tap):
                if (specialType == Note.SpecialType.STAR_POW)
                {
                    materials = resources.spTapRenderer.sharedMaterials;
                    materials[SP_COLOR_MAT_POS] = GetValidMaterial(tapColorMats, fretNumber);
                }
                else
                {
                    materials = resources.tapRenderer.sharedMaterials;
                    materials[STANDARD_COLOUR_MAT_POS] = GetValidMaterial(tapColorMats, fretNumber);
                }
                break;
            default:    // strum
                if (specialType == Note.SpecialType.STAR_POW)
                {
                    materials = resources.spStrumRenderer.sharedMaterials;
                    materials[SP_COLOR_MAT_POS] = GetValidMaterial(strumColorMats, fretNumber);
                }
                else
                {
                    materials = resources.strumRenderer.sharedMaterials;
                    materials[STANDARD_COLOUR_MAT_POS] = GetValidMaterial(strumColorMats, fretNumber);
                }
                break;
        }

        return materials;
    }
}
