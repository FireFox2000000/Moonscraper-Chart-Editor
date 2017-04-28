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
        if (AssignCustomResources.noteSpritesAvaliable != null)
        {
            switch (AssignCustomResources.noteSpritesAvaliable)
            {
                case (Skin.AssestsAvaliable.All):
                    note2D.gameObject.SetActive(true);
                    note3D.gameObject.SetActive(false);
                    enabled = false;
                    break;
                case (Skin.AssestsAvaliable.None):
                    note2D.gameObject.SetActive(false);
                    note3D.gameObject.SetActive(true);
                    enabled = false;
                    break;
                default:
                    break;
            }
        }
    }
	
	// Update is called once per frame
	public void UpdateSelectedGameObject () {
        if (!enabled)
            return;

        Note note = nCon.note;
        Note.Note_Type noteType = NoteVisualsManager.GetTypeWithViewChange(note);
        Note.Special_Type specialType = NoteVisualsManager.IsStarpower(note);

        Texture2D textureInSkin = null;

        if (note != null)
        {
            if (noteType == Note.Note_Type.STRUM)
            {
                if (specialType == Note.Special_Type.STAR_POW)
                    textureInSkin = customSkin.sp_strum[(int)note.fret_type];
                else
                    textureInSkin = customSkin.reg_strum[(int)note.fret_type];
            }
            else if (noteType == Note.Note_Type.HOPO)
            {
                if (specialType == Note.Special_Type.STAR_POW)
                    textureInSkin = customSkin.sp_hopo[(int)note.fret_type];
                else
                    textureInSkin = customSkin.reg_hopo[(int)note.fret_type];
            }
            // Tap notes
            else
            {
                if (note.fret_type != Note.Fret_Type.OPEN)
                {
                    if (specialType == Note.Special_Type.STAR_POW)
                        textureInSkin = customSkin.sp_tap[(int)note.fret_type];
                    else
                        textureInSkin = customSkin.reg_tap[(int)note.fret_type];
                }
            }
        }

        if (textureInSkin)
        {
            note2D.gameObject.SetActive(true);
            note3D.gameObject.SetActive(false);
        }
        else
        {
            note2D.gameObject.SetActive(false);
            note3D.gameObject.SetActive(true);
        }
    }
}
