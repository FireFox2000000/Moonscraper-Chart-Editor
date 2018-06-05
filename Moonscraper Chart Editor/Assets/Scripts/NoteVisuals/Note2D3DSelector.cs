// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note2D3DSelector : MonoBehaviour {
    public NoteController nCon;
    public NoteVisuals2DManager note2D;
    public NoteVisuals3DManager note3D;
    public Skin customSkin;

    public NoteVisualsManager currentVisualsManager
    {
        get
        {
            return note2D.gameObject.activeSelf ? note2D : (NoteVisualsManager)note3D;
        }
    }
    
    void Start()
    {
        /*
        if (AssignCustomResources.noteSpritesAvaliable != null)
        {
            switch (AssignCustomResources.noteSpritesAvaliable)
            {
                //case (Skin.AssestsAvaliable.All):
                    //Set2D();
                    //enabled = false;
                    //break;
                case (Skin.AssestsAvaliable.None):
                    Set3D();
                    //enabled = false;
                    break;
                default:
                    break;
            }
        }*/

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

        Note note = nCon.note;
        Note.NoteType noteType = note.type;
        Note.SpecialType specialType = NoteVisualsManager.IsStarpower(note);

        int arrayPos = GetSpriteArrayPos(note);
        if (Globals.ghLiveMode)
        {
            if (noteType == Note.NoteType.Strum)
            {
                if (specialType == Note.SpecialType.STAR_POW)
                    textureInSkin = customSkin.sp_strum_ghl[arrayPos];
                else
                    textureInSkin = customSkin.reg_strum_ghl[arrayPos];
            }
            else if (noteType == Note.NoteType.Hopo)
            {
                if (specialType == Note.SpecialType.STAR_POW)
                    textureInSkin = customSkin.sp_hopo_ghl[arrayPos];
                else
                    textureInSkin = customSkin.reg_hopo_ghl[arrayPos];
            }
            // Tap notes
            else
            {
                if (!note.IsOpenNote())
                {
                    if (specialType == Note.SpecialType.STAR_POW)
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
                if (specialType == Note.SpecialType.STAR_POW)
                    textureInSkin = customSkin.sp_strum[arrayPos];
                else
                    textureInSkin = customSkin.reg_strum[arrayPos];
            }
            else if (noteType == Note.NoteType.Hopo)
            {
                if (specialType == Note.SpecialType.STAR_POW)
                    textureInSkin = customSkin.sp_hopo[arrayPos];
                else
                    textureInSkin = customSkin.reg_hopo[arrayPos];
            }
            // Tap notes
            else
            {
                if (note.guitarFret != Note.GuitarFret.OPEN)
                {
                    if (specialType == Note.SpecialType.STAR_POW)
                        textureInSkin = customSkin.sp_tap[arrayPos];
                    else
                        textureInSkin = customSkin.reg_tap[arrayPos];
                }
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

            if (note.ghliveGuitarFret >= Note.GHLiveGuitarFret.WHITE_1 && note.ghliveGuitarFret <= Note.GHLiveGuitarFret.WHITE_3)
                arrayPos = 1;
            else if (note.IsOpenNote())
                arrayPos = 2;
        }

        return arrayPos;
    }
}
