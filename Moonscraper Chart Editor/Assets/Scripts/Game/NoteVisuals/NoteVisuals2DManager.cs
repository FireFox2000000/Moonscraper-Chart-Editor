// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteVisuals2DManager : NoteVisualsManager {
    SpriteRenderer ren;
    const float ANIMATION_FRAMERATE = 30;

    static int globalAnimationFrame = 0;
    static int lastUpdatedFrame = -1;

    const float ghlSpriteOffset = 0.4f;

    StringBuilder animationNameString = new StringBuilder(16, 16);

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

            if (Globals.ghLiveMode && !note.IsOpenNote())
                transform.localPosition = new Vector3(0, ghlSpriteOffset, 0);
            else
                transform.localPosition = Vector3.zero;

            if (!Globals.ghLiveMode)
            {
                if (note.guitarFret == Note.GuitarFret.Open)
                    scale = new Vector3(1.2f, 1, 1);
                else if (specialType == Note.SpecialType.StarPower)
                    scale = new Vector3(1.2f, 1.2f, 1);
            }
        }
        transform.localScale = scale;
    }

    void UpdateGlobalAnimationFrame()
    {
        if (Time.frameCount != lastUpdatedFrame)
        {
            // Determine global animation frame for syncronisation
            globalAnimationFrame = (int)(Time.realtimeSinceStartup * ANIMATION_FRAMERATE);
            lastUpdatedFrame = Time.frameCount;
        }
    }

    protected override void Animate()
    {
        base.Animate();

        Note note = nCon.note;

        if (note != null)
        {
            int noteArrayPos = GetNoteArrayPos(note);
            Note.NoteType visualNoteType = noteType;

            if (!Globals.ghLiveMode)
            {
                if (noteType == Note.NoteType.Hopo && Globals.drumMode)
                {
                    visualNoteType = Note.NoteType.Strum;
                }
            }

            Skin skin = SkinManager.Instance.currentSkin;
            string noteKey = GetSkinKey(noteArrayPos, noteType, specialType, Globals.ghLiveMode);
            Sprite[] sprites = skin.GetSprites(noteKey);
            if (sprites != null && sprites.Length > 0)
            {
                UpdateGlobalAnimationFrame();

                // Get anim index
                float animSpeed = 1.0f;
                int alteredGlobalAnimationFrame = (int)(globalAnimationFrame * animSpeed);
                int frame = alteredGlobalAnimationFrame - ((int)(alteredGlobalAnimationFrame / sprites.Length) * sprites.Length);

                // Set the final sprites
                ren.sprite = sprites.Length > 0 ? sprites[frame] : null;
            }
            else
            {
                // Todo, show error sprite
            }
        }
    }

    public static int GetNoteArrayPos(Note note)
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

    static StringBuilder skinKeySb = new StringBuilder();
    public static string GetSkinKey(int notePos, Note.NoteType noteType, Note.SpecialType specialType, bool isGhl)
    {
        skinKeySb.Clear();      // Reuse the same builder to reduce GC allocs
        StringBuilder sb = skinKeySb;

        sb.AppendFormat("{0}_", notePos);

        if (specialType == Note.SpecialType.StarPower)
        {
            sb.AppendFormat("sp_");
        }
        else
        {
            sb.AppendFormat("reg_");
        }

        switch (noteType)
        {
            case Note.NoteType.Strum:
                {
                    sb.AppendFormat("strum");
                    break;
                }
            case Note.NoteType.Hopo:
                {
                    sb.AppendFormat("hopo");
                    break;
                }
            case Note.NoteType.Tap:
                {
                    sb.AppendFormat("tap");
                    break;
                }
            case Note.NoteType.Cymbal:
                {
                    sb.AppendFormat("cymbal");
                    break;
                }
            default:
                break;
        }

        if (isGhl)
        {
            sb.AppendFormat("_ghl");
        }

        return sb.ToString();
    }
}
