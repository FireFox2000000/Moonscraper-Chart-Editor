// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using System.Text;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteVisuals2DManager : NoteVisualsManager {
    SpriteRenderer ren;
    const float ANIMATION_FRAMERATE = 30;

    static int globalAnimationFrame = 0;
    static int lastUpdatedFrame = -1;

    const float ghlSpriteOffset = 0.4f;

    StringBuilder animationNameString = new StringBuilder(16, 16);
    Sprite[] currentAnimationData = null;

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

        currentAnimationData = null;
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
                // This is based on GH3 sprites. 
                // For some reason the open note sprite sheets sourced from Exilelord's GH3+ mod don't fit compared to the rest of the sprites. 
                // It's possible there's some weird GH3 code that are scaling them in a strage way.
                // We're simply correcting for that and making them fit.
                if (note.guitarFret == Note.GuitarFret.Open)
                    scale = new Vector3(1.2f, 1, 1);
                else if (specialType == Note.SpecialType.StarPower)
                    scale = new Vector3(1.2f, 1.2f, 1);
            }

            // We have a note. Here we're figuring out which set of sprites will be used to display that sprite and assigning them to currentAnimationData.
            // The actual setting of the sprite in the renderer will happen later on in the Animate() function
            {
                int noteArrayPos = GetNoteArrayPos(note, ChartEditor.Instance.laneInfo);
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
                currentAnimationData = skin.GetSprites(noteKey);
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

        // Our animation should have already been assigned if we're active and entering here.
        // Now we determine which sprite to show from that animation!
        // All note animations are synced to mimic other games, esp GH3, using a global aniamtion frame
        Sprite[] sprites = currentAnimationData;
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
            // Report error
            ren.sprite = null;
            Debug.LogError("Missing sprite for 2d note");
        }
    }

    public static int GetNoteArrayPos(Note note, LaneInfo laneInfo)    // Note that this isn't actually an arry position but basically an identifier for which colour to show from the custom resources. This used to be an array position before the refactor
    {
        int arrayPos = note.rawNote;

        if (note.ShouldBeCulledFromLanes(laneInfo))     // Should have been culled, but we want to display it anyway, clamp it to the last lane
        {
            arrayPos = Mathf.Min(note.rawNote, laneInfo.laneCount - 1);    // Clamp to the edge of the lanes
        }

        if (Globals.ghLiveMode)
        {
            arrayPos = 0;

            if (note.ghliveGuitarFret >= Note.GHLiveGuitarFret.White1 && note.ghliveGuitarFret <= Note.GHLiveGuitarFret.White3)
                arrayPos = 1;
            else if (note.IsOpenNote())
                arrayPos = 2;
        }
        else if (Globals.drumMode && note.drumPad != Note.DrumPad.Kick)
        {
            arrayPos += 1;
            if (arrayPos > (laneInfo.laneCount - 1))
                arrayPos = 0;
        }        

        return arrayPos;
    }

    public static int GetSkinKeyHash(int notePos, Note.NoteType noteType, Note.SpecialType specialType, bool isGhl)
    {
        int result = 4;
        int salt = 1231;

        result = unchecked(result * salt + notePos);
        result = unchecked(result * salt + (int)noteType);
        result = unchecked(result * salt + (int)specialType);
        result = unchecked(result * salt + (isGhl ? 1 : -1));

        return result;
    }

    static Dictionary<int, string> skinKeySkinCache = new Dictionary<int, string>();
    static StringBuilder skinKeySb = new StringBuilder();
    public static string GetSkinKey(int notePos, Note.NoteType noteType, Note.SpecialType specialType, bool isGhl)
    {
        int hash = GetSkinKeyHash(notePos, noteType, specialType, isGhl);

        string stringKey;
        if (skinKeySkinCache.TryGetValue(hash, out stringKey))
        {
            return stringKey;
        }

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

        stringKey = sb.ToString();
        skinKeySkinCache[hash] = stringKey;

        return stringKey;
    }
}
