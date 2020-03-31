﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note2D3DSelector : MonoBehaviour {
    public NoteController nCon;
    public NoteVisuals2DManager note2D;
    public NoteVisuals3DManager note3D;

    public NoteVisualsManager currentVisualsManager
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
        Texture2D textureInSkin = null;
        Skin customSkin = SkinManager.Instance.currentSkin;

        Note note = nCon.note;
        Note.NoteType noteType = NoteVisualsManager.GetVisualNoteType(note);
        Note.SpecialType specialType = NoteVisualsManager.IsStarpower(note);

        int arrayPos = GetSpriteArrayPos(note);
        if (Globals.ghLiveMode)
        {
            if (noteType == Note.NoteType.Strum)
            {
                if (specialType == Note.SpecialType.StarPower)
                    textureInSkin = customSkin.sp_strum_ghl[arrayPos];
                else
                    textureInSkin = customSkin.reg_strum_ghl[arrayPos];
            }
            else if (noteType == Note.NoteType.Hopo)
            {
                if (specialType == Note.SpecialType.StarPower)
                    textureInSkin = customSkin.sp_hopo_ghl[arrayPos];
                else
                    textureInSkin = customSkin.reg_hopo_ghl[arrayPos];
            }
            // Tap notes
            else
            {
                if (!note.IsOpenNote())
                {
                    if (specialType == Note.SpecialType.StarPower)
                        textureInSkin = customSkin.sp_tap_ghl[arrayPos];
                    else
                        textureInSkin = customSkin.reg_tap_ghl[arrayPos];
                }
            }   
        }
        else
        {
            if (noteType == Note.NoteType.Strum)
            {
                if (specialType == Note.SpecialType.StarPower)
                    textureInSkin = customSkin.sp_strum[arrayPos];
                else
                    textureInSkin = customSkin.reg_strum[arrayPos];
            }
            else if (noteType == Note.NoteType.Hopo)
            {
                if (specialType == Note.SpecialType.StarPower)
                    textureInSkin = customSkin.sp_hopo[arrayPos];
                else
                    textureInSkin = customSkin.reg_hopo[arrayPos];
            }
            // Tap notes
            else if (noteType == Note.NoteType.Tap)
            {
                if (note.guitarFret != Note.GuitarFret.Open)
                {
                    if (specialType == Note.SpecialType.StarPower)
                        textureInSkin = customSkin.sp_tap[arrayPos];
                    else
                        textureInSkin = customSkin.reg_tap[arrayPos];
                }
            }
            else if (noteType == Note.NoteType.Cymbal)
            {
                if (specialType == Note.SpecialType.StarPower)
                    textureInSkin = customSkin.sp_cymbal[arrayPos];
                else
                    textureInSkin = customSkin.reg_cymbal[arrayPos];
            }
        }

        return textureInSkin;
    }

    protected int GetSpriteArrayPos(Note note)
    {
        int arrayPos = note.rawNote;

        if (Globals.ghLiveMode)
        {
            arrayPos = 0;

            if (note.ghliveGuitarFret >= Note.GHLiveGuitarFret.White1 && note.ghliveGuitarFret <= Note.GHLiveGuitarFret.White3)
                arrayPos = 1;
            else if (note.IsOpenNote())
                arrayPos = 2;
        }
        else if (Globals.drumMode && note.guitarFret != Note.GuitarFret.Open)
        {
            arrayPos += 1;
            if (arrayPos > (int)Note.GuitarFret.Orange)
                arrayPos = 0;
        }

        return arrayPos;
    }
}
