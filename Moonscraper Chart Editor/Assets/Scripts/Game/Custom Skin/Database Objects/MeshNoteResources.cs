// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public class MeshNoteResources : ScriptableObject {
    [Header("Note models")]
    public MeshFilter standardModel;
    public MeshFilter spModel;
    public MeshFilter openModel;

    [Header("Note renderers")]
    public Renderer strumRenderer;
    public Renderer hopoRenderer;
    public Renderer tapRenderer;
    public Renderer cymbalRenderer;
    public Renderer openRenderer;
    public Renderer spStrumRenderer;
    public Renderer spHopoRenderer;
    public Renderer spTapRenderer;
    public Renderer spCymbalRenderer;

    [Header("Open note")]
    public Material[] openMaterials = new Material[5];

    [Header("Strum config")]
    public Material[] strumColorPalette = new Material[9];

    [Header("Tap config")]
    public Material[] tapColorPalette = new Material[9];

    [Header("Cymbal config")]
    public Material[] cymbalColorPalette = new Material[9];

    [Header("Starpower config")]
    public Material[] starpowerMaterials = new Material[5];
    public Material[] starpowerDrumFillMaterials = new Material[5];

    [Header("Palette maps")]
    public int[] guitarModeLaneColorIndicies = new int[5];
    public int[] drumModeLaneColorIndicies = new int[5];
    public int[] drumModeLaneColorIndicies4LaneOverride = new int[4];
    public int[] ghlGuitarModeLaneColorIndicies = new int[6];

    public int starpowerLaneColorIndex;
    public int toolNoteLaneColorIndex;

    int[] LookupPaletteMapForGameMode(Chart.GameMode gameMode, LaneInfo laneInfo)
    {
        int[] paletteMap = null;
        int laneCount = laneInfo.laneCount;

        switch (gameMode)
        {
            case (Chart.GameMode.Guitar):
                paletteMap = guitarModeLaneColorIndicies;
                break;
            case (Chart.GameMode.Drums):
                paletteMap = laneCount == 4 ? drumModeLaneColorIndicies4LaneOverride : drumModeLaneColorIndicies;
                break;
            case (Chart.GameMode.GHLGuitar):
                paletteMap = ghlGuitarModeLaneColorIndicies;
                break;
            default:
                throw new System.Exception("Unhandled gamemode");
        }

        return paletteMap;
    }

    public Material GetStrumMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        int[] paletteMap = LookupPaletteMapForGameMode(gameMode, laneInfo);

        return strumColorPalette[paletteMap[noteIndex]];
    }

    public Material GetTapMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        int[] paletteMap = LookupPaletteMapForGameMode(gameMode, laneInfo);

        return tapColorPalette[paletteMap[noteIndex]];
    }

    public Material GetCymbalMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        int[] paletteMap = LookupPaletteMapForGameMode(gameMode, laneInfo);

        return cymbalColorPalette[paletteMap[noteIndex]];
    }

    public Material GetToolStrumMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return strumColorPalette[toolNoteLaneColorIndex];
    }

    public Material GetToolTapMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return tapColorPalette[toolNoteLaneColorIndex];
    }

    public Material GetToolCymbalMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return cymbalColorPalette[toolNoteLaneColorIndex];
    }

    public Material GetStarpowerColorMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return strumColorPalette[starpowerLaneColorIndex];
    }
}
