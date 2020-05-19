// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    bool CheckTextureInSkin()
    {
        Skin customSkin = SkinManager.Instance.currentSkin;

        Note note = nCon.note;
        Note.NoteType noteType = NoteVisualsManager.GetVisualNoteType(note);
        Note.SpecialType specialType = NoteVisualsManager.IsStarpower(note);

        int arrayPos = NoteVisuals2DManager.GetNoteArrayPos(note);
        Note.NoteType visualNoteType = noteType;

        if (!Globals.ghLiveMode)
        {
            if (noteType == Note.NoteType.Hopo && Globals.drumMode)
            {
                visualNoteType = Note.NoteType.Strum;
            }
        }

        string noteKey = NoteVisuals2DManager.GetSkinKey(arrayPos, noteType, specialType, Globals.ghLiveMode);
        Sprite[] sprites = SkinManager.Instance.currentSkin.GetSprites(noteKey);

        return sprites != null && sprites.Length > 0;
    }
}
