using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    protected override void Animate()
    {
        base.Animate();

        SpriteAnimation(nCon.note);
    }

    void SpriteAnimation(Note note)
    {
        if (note != null)
        {
            if (Time.frameCount != lastUpdatedFrame)
            {
                // Determine global animation frame for syncronisation
                globalAnimationFrame = (int)(Time.realtimeSinceStartup * ANIMATION_FRAMERATE);
                lastUpdatedFrame = Time.frameCount;
            }

            // Determine which animation offset data to use
            string animationName = string.Empty;

            if (specialType == Note.Special_Type.STAR_POW)
            {
                animationName += "sp_";
            }
            else
            {
                animationName += "reg_";
            }

            if (note.fret_type == Note.Fret_Type.OPEN)
            {
                animationName = "open_" + animationName;
            }

            if (noteType == Note.Note_Type.HOPO)
                animationName += "hopo";
            else if (noteType == Note.Note_Type.STRUM)
                animationName += "strum";
            else
                animationName += "tap";

            NoteSpriteAnimationData animationData;

            // Search for the animation
            if (animationDataDictionary.TryGetValue(animationName, out animationData))
            { 
                // Get sprite name and number
                string spriteText = ren.sprite.name;
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
                    //Debug.Log(frame + ", " + (animationData.offsets.Length - 1));
                   // Debug.Log(spriteNumber);
                    spriteNumber += animationData.offsets[frame];
                    //Debug.Log(spriteNumber);
                    spriteName += spriteNumber.ToString();
                    //Debug.Log(spriteName);
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
