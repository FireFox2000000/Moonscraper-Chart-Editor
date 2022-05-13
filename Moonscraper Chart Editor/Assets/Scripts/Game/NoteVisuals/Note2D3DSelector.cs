// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class Note2D3DSelector : MonoBehaviour {
    public NoteController nCon;
    public NoteVisuals2DManager note2D;
    public NoteVisuals3DManager note3D;

    NoteVisualsManager currentVisualsManager
    {
        get
        {
            return note2D.gameObject.activeSelf ? note2D : (NoteVisualsManager)note3D;
        }
    }
    
    void Start()
    {
        UpdateSelectedGameObject();
    }

    public void UpdateSelectedGameObject()
    {
        if (CheckTextureInSkin())
            Set2D();
        else
            Set3D();

        currentVisualsManager.UpdateVisuals();
    }

    void Set2D()
    {
        note2D.gameObject.SetActive(true);
        note3D.gameObject.SetActive(false);
    }

    void Set3D()
    {
        note2D.gameObject.SetActive(false);
        note3D.gameObject.SetActive(true);
    }

    static Dictionary<int, bool> textureInSkinCache = new Dictionary<int, bool>();
    bool CheckTextureInSkin()
    {
        Skin customSkin = SkinManager.Instance.currentSkin;

        bool isGhl = Globals.ghLiveMode;
        bool isDrumMode = Globals.drumMode;

        Note note = nCon.note;
        NoteVisualsManager.VisualNoteType noteType = NoteVisualsManager.GetVisualNoteType(note);
        Note.SpecialType specialType = NoteVisualsManager.IsStarpower(note);

        int arrayPos = NoteVisuals2DManager.GetNoteArrayPos(note, ChartEditor.Instance.laneInfo);

        bool isInSkin;
        int hash = NoteVisuals2DManager.GetSkinKeyHash(arrayPos, noteType, specialType, isGhl, isDrumMode);
        
        if (textureInSkinCache.TryGetValue(hash, out isInSkin))
        {
            return isInSkin;
        }
        else
        {
            string noteKey = NoteVisuals2DManager.GetSkinKey(arrayPos, noteType, specialType, isGhl, isDrumMode);
            Sprite[] sprites = SkinManager.Instance.currentSkin.GetSprites(noteKey);

            isInSkin = sprites != null && sprites.Length > 0;
            if (!isInSkin && isDrumMode)
            {
                switch(noteType)
                {
                    case NoteVisualsManager.VisualNoteType.Cymbal:
                        noteKey = NoteVisuals2DManager.GetSkinKey(arrayPos, NoteVisualsManager.VisualNoteType.Tap, specialType, false, false);
                        break;
                    case NoteVisualsManager.VisualNoteType.DoubleKick:
                        noteKey = NoteVisuals2DManager.GetSkinKey(arrayPos, NoteVisualsManager.VisualNoteType.Kick, specialType, false, true);
                        break;
                    case NoteVisualsManager.VisualNoteType.Tom:
                    case NoteVisualsManager.VisualNoteType.Kick:
                        noteKey = NoteVisuals2DManager.GetSkinKey(arrayPos, NoteVisualsManager.VisualNoteType.Strum, specialType, false, false);
                        break;
                    case NoteVisualsManager.VisualNoteType.Strum:
                    case NoteVisualsManager.VisualNoteType.Tap:
                        break;
                }
                sprites = SkinManager.Instance.currentSkin.GetSprites(noteKey);
            }
            isInSkin = sprites != null && sprites.Length > 0;
            textureInSkinCache[hash] = isInSkin;

            return isInSkin;
        }
    }
}
