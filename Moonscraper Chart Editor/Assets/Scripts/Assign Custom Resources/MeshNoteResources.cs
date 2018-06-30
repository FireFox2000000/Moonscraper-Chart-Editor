// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshNoteResources : ScriptableObject {
    [Header("Note models")]
    public MeshFilter standardModel;
    public MeshFilter spModel;
    public MeshFilter openModel;

    [Header("Note renderers")]
    public Renderer strumRenderer;
    public Renderer hopoRenderer;
    public Renderer tapRenderer;
    public Renderer openRenderer;
    public Renderer spStrumRenderer;
    public Renderer spHopoRenderer;
    public Renderer spTapRenderer;

    [Header("Note colours")]
    public Material[] strumColors = new Material[6];
    public Material[] tapColors = new Material[5];

    public Material spTemp;
    public Material spTapTemp;

    [Header("Open note")]
    public Material[] openMaterials = new Material[5];

    [Header("GHLive Note colours")]
    public Material[] ghlStrumColors = new Material[2];
    public Material[] ghlTapColors = new Material[2];

    [Header("Strum config")]
    public Material[] strumColorPalette = new Material[9];

    [Header("Tap config")]
    public Material[] tapColorPalette = new Material[9];

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

    public Material GetToolStrumMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return strumColorPalette[toolNoteLaneColorIndex];
    }

    public Material GetToolTapMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return tapColorPalette[toolNoteLaneColorIndex];
    }

    public Material GetStarpowerColorMaterial(Chart.GameMode gameMode, LaneInfo laneInfo, int noteIndex)
    {
        return strumColorPalette[starpowerLaneColorIndex];
    }
}
