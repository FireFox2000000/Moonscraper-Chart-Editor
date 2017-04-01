using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteVisuals2DManager : NoteVisualsManager {
    SpriteRenderer ren;
    public SpriteNoteResources spriteResources;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        ren = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    public override void UpdateVisuals()
    {
        base.UpdateVisuals();

        Note note = nCon.note;

        Vector3 scale = new Vector3(1, 1, 1);
        if (note != null)
        {
            if (noteType == Note.Note_Type.STRUM)
            {
                if (specialType == Note.Special_Type.STAR_POW)
                    ren.sprite = spriteResources.sp_strum[(int)note.fret_type];
                else
                    ren.sprite = spriteResources.reg_strum[(int)note.fret_type];
            }
            else if (noteType == Note.Note_Type.HOPO)
            {
                if (specialType == Note.Special_Type.STAR_POW)
                    ren.sprite = spriteResources.sp_hopo[(int)note.fret_type];
                else
                    ren.sprite = spriteResources.reg_hopo[(int)note.fret_type];
            }
            // Tap notes
            else
            {
                if (note.fret_type != Note.Fret_Type.OPEN)
                {
                    if (specialType == Note.Special_Type.STAR_POW)
                        ren.sprite = spriteResources.sp_tap[(int)note.fret_type];
                    else
                        ren.sprite = spriteResources.reg_tap[(int)note.fret_type];
                }
            }

            if (note.fret_type == Note.Fret_Type.OPEN)
                scale = new Vector3(1.2f, 1, 1);
            else if (specialType == Note.Special_Type.STAR_POW)
                scale = new Vector3(1.2f, 1.2f, 1);
        }

        transform.localScale = scale;
    }
}
