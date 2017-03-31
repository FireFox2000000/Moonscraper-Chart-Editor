using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note2D3DSelector : MonoBehaviour {
    public NoteController nCon;
    public GameObject note2D;
    public GameObject note3D;
    public Skin customSkin;
	
	// Update is called once per frame
	void Update () {
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
            note2D.SetActive(true);
            note3D.SetActive(false);
        }
        else
        {
            note2D.SetActive(false);
            note3D.SetActive(true);
        }
    }
}
