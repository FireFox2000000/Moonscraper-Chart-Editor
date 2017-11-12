// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public GameplayManager break0;
    public StrikelineAudioController clap;
    public Renderer[] background = new Renderer[2];
    public Renderer fretboard;
    public Metronome metronome;

    Texture initBGTex;
    Texture initFretboardTex;
    public Skin customSkin;
    public SpriteNoteResources defaultNoteSprites;
    public CustomFretManager[] customFrets = new CustomFretManager[5];

    public static Skin.AssestsAvaliable? noteSpritesAvaliable = null;

    // Use this for initialization
    void Awake () {
        initBGTex = background[0].sharedMaterial.mainTexture;
        initFretboardTex = fretboard.sharedMaterial.mainTexture;

        try
        {
            if (customSkin.break0 != null)
                break0.comboBreak = customSkin.break0;
            if (customSkin.clap != null)
                clap.clap = customSkin.clap;

            if (customSkin.backgrounds.Length > 0)
            {
                foreach (Renderer bg in background)
                    bg.sharedMaterial.mainTexture = customSkin.backgrounds[0];
            }
            if (customSkin.fretboard != null)
                fretboard.sharedMaterial.mainTexture = customSkin.fretboard;
            if (customSkin.metronome != null)
                metronome.clap = customSkin.metronome;

            WriteCustomNoteTexturesToAtlus(defaultNoteSprites.fullAtlus);
            WriteCustomGHLNoteTexturesToAtlus(defaultNoteSprites.fullAtlusGhl);
            Debug.Log(noteSpritesAvaliable);
            GenerateAndAssignFretSprites();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void setSpriteTextures(Sprite[] sprites, Texture2D[] customTextures)
    {
        for (int i = 0; i < customTextures.Length; ++i)
        {
            if (i < sprites.Length && customTextures[i] && sprites[i]/* && customTextures[i].width == sprites[i].texture.width && customTextures[i].height == sprites[i].texture.height*/)
            {
                sprites[i].texture.SetPixels(customTextures[i].GetPixels());
                sprites[i].texture.Apply();
            }
        }
    }

    void WriteCustomNoteTexturesToAtlus(Texture2D atlus)
    {
        Color[] atlusPixels = atlus.GetPixels();
        Utility.IntVector2 fullTextureAtlusSize = new Utility.IntVector2(atlus.width, atlus.height);

        SetCustomTexturesToAtlus(defaultNoteSprites.reg_strum, customSkin.reg_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_hopo, customSkin.reg_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_tap, customSkin.reg_tap, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_strum, customSkin.sp_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_hopo, customSkin.sp_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_tap, customSkin.sp_tap, atlusPixels, fullTextureAtlusSize);

        Skin.AssestsAvaliable? sprites = noteSpritesAvaliable;
        SetCustomTexturesToAtlus(defaultNoteSprites.sustains, customSkin.sustains, atlusPixels, fullTextureAtlusSize);
        noteSpritesAvaliable = sprites;

        atlus.SetPixels(atlusPixels);
        atlus.Apply();
    }

    void WriteCustomGHLNoteTexturesToAtlus(Texture2D atlus)
    {
        Color[] atlusPixels = atlus.GetPixels();
        Utility.IntVector2 fullTextureAtlusSize = new Utility.IntVector2(atlus.width, atlus.height);

        SetCustomTexturesToAtlus(defaultNoteSprites.reg_strum_ghl, customSkin.reg_strum_ghl, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_hopo_ghl, customSkin.reg_hopo_ghl, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_tap_ghl, customSkin.reg_tap_ghl, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_strum_ghl, customSkin.sp_strum_ghl, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_hopo_ghl, customSkin.sp_hopo_ghl, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_tap_ghl, customSkin.sp_tap_ghl, atlusPixels, fullTextureAtlusSize);

        atlus.SetPixels(atlusPixels);
        atlus.Apply();
    }

    static void SetCustomTexturesToAtlus(Sprite[] spritesLocation, Texture2D[] customTextures, Color[] fullTextureAtlusPixels, Utility.IntVector2 fullTextureAtlusSize)
    {
        if (spritesLocation.Length != customTextures.Length)
            throw new System.Exception("Mis-aligned sprite locations to textures provided");

        for (int i = 0; i < customTextures.Length; ++i)
        {
            if (customTextures[i] && spritesLocation[i])
            {
                if (noteSpritesAvaliable == null)
                    noteSpritesAvaliable = Skin.AssestsAvaliable.All;
                else if (noteSpritesAvaliable == Skin.AssestsAvaliable.None)
                    noteSpritesAvaliable = Skin.AssestsAvaliable.Mixed;

                try
                {
                    WritePixelsToArea(customTextures[i].GetPixels(),
                        new Utility.IntVector2(customTextures[i].width, customTextures[i].height),
                        new Utility.IntVector2((int)(spritesLocation[i].rect.xMin), (int)(spritesLocation[i].texture.height - spritesLocation[i].rect.yMax)),
                        fullTextureAtlusPixels, fullTextureAtlusSize);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    customTextures[i] = null;
                }
            }
            else if (!customTextures[i])
            {
                if (noteSpritesAvaliable == null)
                    noteSpritesAvaliable = Skin.AssestsAvaliable.None;
                else if (noteSpritesAvaliable == Skin.AssestsAvaliable.All)
                    noteSpritesAvaliable = Skin.AssestsAvaliable.Mixed;
            }
        }
    }

    static void WritePixelsToArea(Color[] texturePixels, Utility.IntVector2 textureSize, Utility.IntVector2 topLeftCornerToStartWriteFrom, Color[] pixelsToOverwrite, Utility.IntVector2 textureToWriteSize)
    {
        if (textureSize.x * textureSize.y != texturePixels.Length)
            throw new System.Exception("Invalid texture size.");
        
        if (topLeftCornerToStartWriteFrom.x + textureSize.x > textureToWriteSize.x || topLeftCornerToStartWriteFrom.y + textureSize.y > textureToWriteSize.y)
            throw new System.Exception("Invalid texture write location.");

        for (int j = 0; j < textureSize.y; ++j)
        {
            for (int i = 0; i < textureSize.x; ++i)
            {
                pixelsToOverwrite[(textureToWriteSize.y - topLeftCornerToStartWriteFrom.y - textureSize.y + j) * textureToWriteSize.x + topLeftCornerToStartWriteFrom.x + i] = texturePixels[j * textureSize.x + i];
            }
        }
    }
	
    static Sprite MakeFretSprite(Texture2D fret)
    {
        const int PIXELS_PER_UNIT = 125;
        return Sprite.Create(fret, new Rect(0.0f, 0.0f, fret.width, fret.height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);
    }

    void GenerateAndAssignFretSprites()
    {
        const int PIXELS_PER_UNIT = 125;

        for (int i = 0; i < customFrets.Length; ++i)
        {
            // Standard Frets
            if (i < customSkin.fret_base.Length)
            {
                //Sprite sprite = null;
                if (customSkin.fret_base[i])
                {
                    customFrets[i].fretBase = MakeFretSprite(customSkin.fret_base[i]);
                    customFrets[i].gameObject.SetActive(true);
                }

                //customFrets[i].fretBaseRen.sprite = sprite;
            }
            
            if (i < customSkin.fret_cover.Length)
            {
                if (customSkin.fret_cover[i])
                {
                    customFrets[i].fretCover = MakeFretSprite(customSkin.fret_cover[i]);
                    customFrets[i].gameObject.SetActive(true);
                }
            }

            if (i < customSkin.fret_press.Length)
            {
                if (customSkin.fret_press[i])
                {
                    customFrets[i].fretPress = MakeFretSprite(customSkin.fret_press[i]);
                    customFrets[i].gameObject.SetActive(true);
                }
            }

            if (i < customSkin.fret_release.Length)
            {
                if (customSkin.fret_release[i])
                {
                    customFrets[i].fretRelease = MakeFretSprite(customSkin.fret_release[i]);
                    customFrets[i].gameObject.SetActive(true);
                }
            }

            if (i < customSkin.fret_anim.Length)
            {
                if (customSkin.fret_anim[i])
                {
                    customFrets[i].toAnimate = MakeFretSprite(customSkin.fret_anim[i]);
                    customFrets[i].gameObject.SetActive(true);
                }
            }

            // Drum Frets
            
            if (i < customSkin.drum_fret_base.Length && customSkin.drum_fret_base[i])
                customFrets[i].drumFretBase = MakeFretSprite(customSkin.drum_fret_base[i]);

            if (i < customSkin.drum_fret_cover.Length && customSkin.drum_fret_cover[i])
                customFrets[i].drumFretCover = MakeFretSprite(customSkin.drum_fret_cover[i]);

            if (i < customSkin.drum_fret_press.Length && customSkin.drum_fret_press[i])
                customFrets[i].drumFretPress = MakeFretSprite(customSkin.drum_fret_press[i]);

            if (i < customSkin.drum_fret_release.Length && customSkin.drum_fret_release[i])
                customFrets[i].drumFretRelease = MakeFretSprite(customSkin.drum_fret_release[i]);

            if (i < customSkin.drum_fret_anim.Length && customSkin.drum_fret_anim[i])
                customFrets[i].drumToAnimate = MakeFretSprite(customSkin.drum_fret_anim[i]);
        }

        Sprite stem = null;
        if (customSkin.fret_stem)
        {
            stem = Sprite.Create(customSkin.fret_stem, new Rect(0.0f, 0.0f, customSkin.fret_stem.width, customSkin.fret_stem.height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);
            for (int i = 0; i < customFrets.Length; ++i)
            {
                customFrets[i].fretStemRen.sprite = stem;
            }
        }
    }

    void OnApplicationQuit()
    {
        // This is purely for the sake of editor resetting, otherwise any custom textures used will be saved between testing
#if UNITY_EDITOR
        foreach (Renderer bg in background)
            bg.sharedMaterial.mainTexture = initBGTex;
        fretboard.sharedMaterial.mainTexture = initFretboardTex;

        // Reset after play
        for (int i = 0; i < customSkin.sustain_mats.Length; ++i)
        {
            if (customSkin.sustain_mats[i])
            {
                Destroy(customSkin.sustain_mats[i]);
                customSkin.sustain_mats[i] = null;
            }
        }
#endif
    }
}
