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
}

public class SkinManager : UnitySingleton<SkinManager> {
    class Skin
    {
        Dictionary<string, UnityEngine.Object> m_skinObjects = new Dictionary<string, UnityEngine.Object>();
        // Todo, sustain materials etc

        public T GetSkinItem<T>(string key, T defaultItem) where T : UnityEngine.Object
        {
            T skinItem = null;

            UnityEngine.Object outObject;
            if (m_skinObjects.TryGetValue(key, out outObject))
                skinItem = outObject as T;

            if (!skinItem)
                skinItem = defaultItem;

            return skinItem;
        }

        public void AddSkinItem<T>(string key, T skinItem) where T : UnityEngine.Object
        {
            if (skinItem)
                m_skinObjects.Add(key, skinItem);
        }
    }

    Skin m_currentSkin = new Skin();

    public T GetSkinItem<T>(string key, T defaultItem) where T : UnityEngine.Object
    {
        return m_currentSkin.GetSkinItem(key, defaultItem);
    }

    public void AddSkinItem<T>(string key, T skinItem) where T : UnityEngine.Object
    {
        m_currentSkin.AddSkinItem(key, skinItem);
    }

    public void ApplySkin()
    {
        // Todo, find all objects that can switch skins and send events to those to update/refresh

    }
}
