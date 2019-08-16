// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteVisuals2DManager : NoteVisualsManager {
    SpriteRenderer ren;
    public SpriteNoteResources spriteResources;
    public NoteSpriteAnimations animations;
    const float ANIMATION_FRAMERATE = 30;

    static Dictionary<string, Sprite> spritesDictionary = null;
    static Dictionary<string, NoteSpriteAnimationData> animationDataDictionary = null;

    static int globalAnimationFrame = 0;
    static int lastUpdatedFrame = -1;

    const float ghlSpriteOffset = 0.4f;

    Sprite lastUpdatedSprite = null;
    StringBuilder animationNameString = new StringBuilder(16, 16);

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        ren = GetComponent<SpriteRenderer>();

        if (spritesDictionary == null)
        {
            spritesDictionary = new Dictionary<string, Sprite>();

            foreach (Sprite sprite in Resources.LoadAll<Sprite>(spriteResources.fullAtlus.name))
            {
                spritesDictionary.Add(sprite.name, sprite);
            }
        }

        if (animationDataDictionary == null)
        {
            animationDataDictionary = new Dictionary<string, NoteSpriteAnimationData>();
            foreach (NoteSpriteAnimationData animationData in animations.animations)
            {
                animationDataDictionary.Add(animationData.name, animationData);
            }
        }
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

            if (Globals.ghLiveMode)
            {
                int noteArrayPos = 0;

                if (note.ghliveGuitarFret >= Note.GHLiveGuitarFret.White1 && note.ghliveGuitarFret <= Note.GHLiveGuitarFret.White3)
                    noteArrayPos = 1;
                else if (note.IsOpenNote())
                    noteArrayPos = 2;

                if (noteType == Note.NoteType.Strum)
                {
                    if (specialType == Note.SpecialType.StarPower)
                        ren.sprite = spriteResources.sp_strum_ghl[noteArrayPos];
                    else
                        ren.sprite = spriteResources.reg_strum_ghl[noteArrayPos];
                }
                else if (noteType == Note.NoteType.Hopo)
                {
                    if (specialType == Note.SpecialType.StarPower)
                        ren.sprite = spriteResources.sp_hopo_ghl[noteArrayPos];
                    else
                        ren.sprite = spriteResources.reg_hopo_ghl[noteArrayPos];
                }
                else
                {
                    if (!note.IsOpenNote())
                    {
                        if (specialType == Note.SpecialType.StarPower)
                            ren.sprite = spriteResources.sp_tap_ghl[noteArrayPos];
                        else
                            ren.sprite = spriteResources.reg_tap_ghl[noteArrayPos];
                    }
                }
            }
            else
            {
                int noteArrayPos = (int)note.guitarFret;
                if (Globals.drumMode && note.guitarFret != Note.GuitarFret.Open)
                {
                    noteArrayPos += 1;
                    if (noteArrayPos > (int)Note.GuitarFret.Orange)
                        noteArrayPos = 0;
                }

                if (noteType == Note.NoteType.Strum || (noteType == Note.NoteType.Hopo && Globals.drumMode))
                {
                    if (specialType == Note.SpecialType.StarPower)
                        ren.sprite = spriteResources.sp_strum[noteArrayPos];
                    else
                        ren.sprite = spriteResources.reg_strum[noteArrayPos];
                }
                else if (noteType == Note.NoteType.Hopo)
                {
                    if (specialType == Note.SpecialType.StarPower)
                        ren.sprite = spriteResources.sp_hopo[noteArrayPos];
                    else
                        ren.sprite = spriteResources.reg_hopo[noteArrayPos];
                }
                // Tap notes
                else
                {
                    if (note.guitarFret != Note.GuitarFret.Open)
                    {
                        if (specialType == Note.SpecialType.StarPower)
                            ren.sprite = spriteResources.sp_tap[noteArrayPos];
                        else
                            ren.sprite = spriteResources.reg_tap[noteArrayPos];
                    }
                }

                if (note.guitarFret == Note.GuitarFret.Open)
                    scale = new Vector3(1.2f, 1, 1);
                else if (specialType == Note.SpecialType.StarPower)
                    scale = new Vector3(1.2f, 1.2f, 1);
            }
        }
        lastUpdatedSprite = ren.sprite;
        transform.localScale = scale;
    }

    protected override void Animate()
    {
        base.Animate();

        if (!Globals.ghLiveMode)
            SpriteAnimation(nCon.note);
    }

    void SpriteAnimation(Note note)
    {
        if (note != null && lastUpdatedSprite != null)
        {
            if (Time.frameCount != lastUpdatedFrame)
            {
                // Determine global animation frame for syncronisation
                globalAnimationFrame = (int)(Time.realtimeSinceStartup * ANIMATION_FRAMERATE);
                lastUpdatedFrame = Time.frameCount;
            }

            // Determine which animation offset data to use
            //string animationName = string.Empty;
            animationNameString.Length = 0;

            if (specialType == Note.SpecialType.StarPower)
            {
                animationNameString.Append("sp_");
            }
            else
            {
                animationNameString.Append("reg_");
            }

            if (note.guitarFret == Note.GuitarFret.Open)
            {
                animationNameString.Insert(0, "open_");
                //animationName = "open_" + animationName;
            }

            if (noteType == Note.NoteType.Hopo && !Globals.drumMode)
                animationNameString.Append("hopo");
            else if (noteType == Note.NoteType.Tap)
                animationNameString.Append("tap");
            else
                animationNameString.Append("strum");

            NoteSpriteAnimationData animationData;

            // Search for the animation
            if (animationDataDictionary.TryGetValue(animationNameString.ToString(), out animationData))
            { 
                // Get sprite name and number
                string spriteText = lastUpdatedSprite.name;
                string spriteName = string.Empty;
                int spriteNumber = -1;

                for (int i = spriteText.Length - 1; i >= 0; --i)
                {
                    if (spriteText[i] == '_')
                    {
                        spriteName = spriteText.Remove(i + 1);
                        spriteNumber = int.Parse(spriteText.Remove(0, i + 1));
                        break;
                    }
                }
                
                if (spriteNumber != -1)
                {
                    int alteredGlobalAnimationFrame = (int)(globalAnimationFrame * animationData.speed);
                    // Change sprite number
                    int frame = alteredGlobalAnimationFrame - ((int)(alteredGlobalAnimationFrame / animationData.offsets.Length) * animationData.offsets.Length);
                    spriteNumber += animationData.offsets[frame];
                    spriteName += spriteNumber.ToString();

                    Sprite newSprite;
                    if (spritesDictionary.TryGetValue(spriteName, out newSprite))
                    {                       
                        ren.sprite = newSprite;
                    }
                }
            }
        }
    }
}
