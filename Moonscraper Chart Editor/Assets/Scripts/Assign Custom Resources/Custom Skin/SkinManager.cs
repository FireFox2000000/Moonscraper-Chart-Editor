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
}

public class SkinManager : UnitySingleton<SkinManager> {
    Skin m_currentSkin = new Skin();
    public Skin currentSkin { get { return m_currentSkin; } set { m_currentSkin = value; } }
    public Skin.AssestsAvaliable? noteSpritesAvaliable = null;

    public T GetSkinItem<T>(string key, T defaultItem) where T : UnityEngine.Object
    {
        return m_currentSkin.GetSkinItem(key, defaultItem);
    }

    public void ApplySkin()
    {
        // Todo, find all objects that can switch skins and send events to those to update/refresh

    }
}
