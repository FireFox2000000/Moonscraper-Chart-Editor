// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkinKeys
{
    public const string clap = "clap";
    public const string metronome = "metronome";
    public const string break0 = "break-0";

    public const string fretboard = "fretboard-0";
    public const string backgroundX = "background-";

    public const string fretStem = "fret_stem";
    public const string hitFlames = "hit_flames";

    public const string xFretBase = "_fret_base";
    public const string xFretCover = "_fret_cover";
    public const string xFretPress = "_fret_press";
    public const string xFretRelease = "_fret_release";
    public const string xFretAnim = "_fret_anim";

    public const string xDrumFretBase = "_drum_fret_base";
    public const string xDrumFretCover = "_drum_fret_cover";
    public const string xDrumFretPress = "_drum_fret_press";
    public const string xDrumFretRelease = "_drum_fret_release";
    public const string xDrumFretAnim = "_drum_fret_anim";

    public const string xFretBaseGhl = "_fret_base_ghl";
    public const string xFretPressGhl = "_fret_press_ghl";

    public const string measureBeatLine = "beat_line_measure";
    public const string standardBeatLine = "beat_line_standard";
    public const string quarterBeatLine = "beat_line_quarter";
}

[UnitySingleton(UnitySingletonAttribute.Type.CreateOnNewGameObject, false)]
public class SkinManager : UnitySingleton<SkinManager>
{
    Skin m_currentSkin = new Skin();
    public Skin currentSkin { get { return m_currentSkin; } set { m_currentSkin = value; } }
    public Skin.AssestsAvaliable? noteSpritesAvaliable = null;

    public T GetSkinItem<T>(string key, T defaultItem) where T : UnityEngine.Object
    {
        return m_currentSkin.GetSkinItem(key, defaultItem);
    }

    public string GetSkinItemFilepath(string key)
    {
        return m_currentSkin.GetSkinItemFilepath(key);
    }

    public void ApplySkin()
    {
        // Todo, find all objects that can switch skins and send events to those to update/refresh

    }
}
