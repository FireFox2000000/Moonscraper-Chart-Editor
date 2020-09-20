// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class NoteVisuals3DManager : NoteVisualsManager
{
    MeshFilter meshFilter;
    public MeshNoteResources resources;

    Material[] resourceSharedMatsStrum;
    Material[] resourceSharedMatsSpStrum;
    Material[] resourceSharedMatsHopo;
    Material[] resourceSharedMatsSpHopo;
    Material[] resourceSharedMatsTap;
    Material[] resourceSharedMatsSpTap;
    Material[] resourceSharedMatsCymbal;
    Material[] resourceSharedMatsSpCymbal;

    // Use this for initialization
    protected override void Awake ()
    {
        base.Awake();
        meshFilter = GetComponent<MeshFilter>();

        resourceSharedMatsStrum = resources.strumRenderer.sharedMaterials;
        resourceSharedMatsSpStrum = resources.spStrumRenderer.sharedMaterials;
        resourceSharedMatsHopo = resources.hopoRenderer.sharedMaterials;
        resourceSharedMatsSpHopo = resources.spHopoRenderer.sharedMaterials;
        resourceSharedMatsTap = resources.tapRenderer.sharedMaterials;
        resourceSharedMatsSpTap = resources.spTapRenderer.sharedMaterials;
        resourceSharedMatsCymbal = resources.cymbalRenderer.sharedMaterials;
        resourceSharedMatsSpCymbal = resources.spCymbalRenderer.sharedMaterials;
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

            ChartEditor editor = ChartEditor.Instance;
            Chart.GameMode gameMode = editor.currentGameMode;
            Note.NoteType visualNoteType = noteType;

            Vector3 scale = new Vector3(1, 1, 1);

            // Determine materials
            if (note.IsOpenNote())
            {
                materials = resources.openRenderer.sharedMaterials;

                int colourIndex = 0;

                if (specialType == Note.SpecialType.StarPower)
                {
                    if (visualNoteType == Note.NoteType.Hopo)
                        colourIndex = 3;
                    else
                        colourIndex = 2;
                }
                else
                {
                    if (visualNoteType == Note.NoteType.Hopo)
                        colourIndex = 1;
                    else
                        colourIndex = isTool ? 4 : 0;
                }

                materials[2] = resources.openMaterials[colourIndex];
            }
            else
            {
                LaneInfo laneInfo = editor.laneInfo;
                Material colorMat;

                int noteIndex = note.rawNote;

                if (note.ShouldBeCulledFromLanes(laneInfo))     // Should have been culled, but we want to display it anyway, clamp it to the last lane
                {
                    noteIndex = Mathf.Min(note.rawNote, laneInfo.laneCount - 1);    // Clamp to the edge of the lanes
                }

                if (isTool)
                {
                    switch (visualNoteType)
                    {
                        case Note.NoteType.Tap:
                            {
                                colorMat = resources.GetToolTapMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                        case Note.NoteType.Cymbal:
                            {
                                colorMat = resources.GetToolCymbalMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                        default:
                            {
                                colorMat = resources.GetToolStrumMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                    }
                }
                else
                {
                    switch (visualNoteType)
                    {
                        case Note.NoteType.Tap:
                            {
                                colorMat = resources.GetTapMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                        case Note.NoteType.Cymbal:
                            {
                                colorMat = resources.GetCymbalMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                        default:
                            {
                                colorMat = resources.GetStrumMaterial(gameMode, laneInfo, noteIndex);
                                break;
                            }
                    }
                }

                materials = GetMaterials(colorMat, visualNoteType);
            }

            transform.localScale = scale;
            noteRenderer.sharedMaterials = materials;
        }

        UpdateTextDisplay(note);
    }

    Material[] GetMaterials(Material colorMat, Note.NoteType visualNoteType)
    {
        Material[] materials;
        const int STANDARD_COLOUR_MAT_POS = 1;
        const int SP_COLOR_MAT_POS = 3;

        bool isStarpower = specialType == Note.SpecialType.StarPower;

        int colorMatIndex = isStarpower ? SP_COLOR_MAT_POS : STANDARD_COLOUR_MAT_POS;

        switch (visualNoteType)
        {
            case Note.NoteType.Hopo:
                materials = isStarpower ? resourceSharedMatsSpHopo : resourceSharedMatsHopo;
                break;

            case Note.NoteType.Tap:
                materials = isStarpower ? resourceSharedMatsSpTap : resourceSharedMatsTap;
                break;

            case Note.NoteType.Cymbal:
                materials = isStarpower ? resourceSharedMatsSpCymbal : resourceSharedMatsCymbal;
                break;

            default:
                materials = isStarpower ? resourceSharedMatsSpStrum : resourceSharedMatsStrum;
                break;
        }

        materials[colorMatIndex] = colorMat;

        return materials;
    }
}
