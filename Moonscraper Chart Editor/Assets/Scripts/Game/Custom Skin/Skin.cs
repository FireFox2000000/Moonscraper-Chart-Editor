// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin {
    // Holds misc resources such as individual textures and audio clips
    Dictionary<string, UnityEngine.Object> m_skinObjects = new Dictionary<string, UnityEngine.Object>();

    // This is used to track the original filepaths of the custom assets. Used for allowing the audio system to load streams directly from files rather than use the audioclips
    Dictionary<string, string> m_filepaths = new Dictionary<string, string>();

    // Holds a set of sprites that have been custom-packed onto a single texture to reduce draw calls
    Dictionary<string, Sprite[]> m_spriteAnimDict = new Dictionary<string, Sprite[]>();

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

    public string GetSkinItemFilepath(string key)
    {
        string path = string.Empty;
        m_filepaths.TryGetValue(key, out path);
        return path;
    }

    public void AddSkinItem<T>(string key, string filepath, T skinItem) where T : UnityEngine.Object
    {
        if (skinItem)
        {
            m_filepaths.Add(key, filepath);
            m_skinObjects.Add(key, skinItem);
        }
    }

    public void SetSpriteSheet(Dictionary<string, Sprite[]> animDict)
    {
        m_spriteAnimDict = animDict;
    }

    public Sprite GetSprite(string key)
    {
        Sprite[] sprites = null;
        if (m_spriteAnimDict.TryGetValue(key, out sprites) && sprites.Length > 0)
        {
            return sprites[0];
        }
        return null;
    }

    public Sprite[] GetSprites(string animKey)
    {
        Sprite[] sprites = null;
        m_spriteAnimDict.TryGetValue(animKey, out sprites);
        return sprites;
    }

    public Texture2D[] sustains = new Texture2D[5];
    public Material[] sustain_mats = new Material[6];

    public enum AssestsAvaliable
    {
        None, All, Mixed
    }
}
