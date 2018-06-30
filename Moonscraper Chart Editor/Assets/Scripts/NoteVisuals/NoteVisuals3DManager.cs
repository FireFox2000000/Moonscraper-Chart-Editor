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
            else if (specialType == Note.SpecialType.StarPower)
                meshFilter.sharedMesh = resources.spModel.sharedMesh;
            else
                meshFilter.sharedMesh = resources.standardModel.sharedMesh;

            Material[] materials;

            // Determine materials
            if (note.IsOpenNote())
            {
                materials = resources.openRenderer.sharedMaterials;

                int colourIndex = 0;

                if (specialType == Note.SpecialType.StarPower)
                {
                    if (noteType == Note.NoteType.Hopo && !Globals.drumMode)
                        colourIndex = 3;
                    else
                        colourIndex = 2;
                }
                else
                {
                    if (noteType == Note.NoteType.Hopo && !Globals.drumMode)
                        colourIndex = 1;
                    else
                        colourIndex = isTool ? 4 : 0;
                }

                materials[2] = resources.openMaterials[colourIndex];
            }
            else
            {
                ChartEditor editor = ChartEditor.GetInstance();
                Chart.GameMode gameMode = editor.currentGameMode;
                LaneInfo laneInfo = editor.laneInfo;

                Material colorMat;

                if (isTool)
                {
                    if (noteType == Note.NoteType.Tap)
                        colorMat = resources.GetToolTapMaterial(gameMode, laneInfo, note.rawNote);
                    else
                        colorMat = resources.GetToolStrumMaterial(gameMode, laneInfo, note.rawNote);
                }
                else
                {
                    if (noteType == Note.NoteType.Tap)
                        colorMat = resources.GetTapMaterial(gameMode, laneInfo, note.rawNote);
                    else
                        colorMat = resources.GetStrumMaterial(gameMode, laneInfo, note.rawNote);
                }

                materials = GetMaterials(colorMat);
            }

            noteRenderer.sharedMaterials = materials;
        }
    }

    Material[] GetMaterials(Material colorMat)
    {
        Material[] materials;
        const int STANDARD_COLOUR_MAT_POS = 1;
        const int SP_COLOR_MAT_POS = 3;

        bool isStarpower = specialType == Note.SpecialType.StarPower;

        int colorMatIndex = isStarpower ? SP_COLOR_MAT_POS : STANDARD_COLOUR_MAT_POS;

        switch (noteType)
        {
            case (Note.NoteType.Hopo):
                materials = isStarpower ? resources.spHopoRenderer.sharedMaterials : resources.hopoRenderer.sharedMaterials;
                break;

            case (Note.NoteType.Tap):
                materials = isStarpower ? resources.spTapRenderer.sharedMaterials : resources.tapRenderer.sharedMaterials;
                break;

            default:
                materials = isStarpower ? resources.spStrumRenderer.sharedMaterials : resources.strumRenderer.sharedMaterials;
                break;
        }

        materials[colorMatIndex] = colorMat;

        return materials;
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
            case (Note.GHLiveGuitarFret.White1):
            case (Note.GHLiveGuitarFret.White2):
            case (Note.GHLiveGuitarFret.White3):
                fretNumber = 1;
                break;

            case (Note.GHLiveGuitarFret.Black1):
            case (Note.GHLiveGuitarFret.Black2):
            case (Note.GHLiveGuitarFret.Black3):
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

                if (specialType == Note.SpecialType.StarPower)
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
                if (specialType == Note.SpecialType.StarPower)
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
                if (specialType == Note.SpecialType.StarPower)
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
