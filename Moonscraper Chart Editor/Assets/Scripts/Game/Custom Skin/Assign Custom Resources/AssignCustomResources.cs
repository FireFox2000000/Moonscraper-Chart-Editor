// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public Renderer fretboard;

    Texture initFretboardTex;
    public SpriteNoteResources defaultNoteSprites;
    public CustomFretManager[] customFrets = new CustomFretManager[5];
    public GHLHitAnimation[] customFretsGHL = new GHLHitAnimation[2];

    // Use this for initialization
    void Awake () {
        initFretboardTex = fretboard.sharedMaterial.mainTexture;

        try
        {
            fretboard.sharedMaterial.mainTexture = SkinManager.Instance.GetSkinItem(SkinKeys.fretboard, initFretboardTex);

            WriteCustomNoteTexturesToAtlus(defaultNoteSprites.fullAtlus);
            WriteCustomGHLNoteTexturesToAtlus(defaultNoteSprites.fullAtlusGhl);
            Debug.Log(SkinManager.Instance.noteSpritesAvaliable);
            AssignFretSprites();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void SetSpriteTextures(Sprite[] sprites, Texture2D[] customTextures)
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
        Skin customSkin = SkinManager.Instance.currentSkin;
        Color[] atlusPixels = atlus.GetPixels();
        Utility.IntVector2 fullTextureAtlusSize = new Utility.IntVector2(atlus.width, atlus.height);

        SetCustomTexturesToAtlus(defaultNoteSprites.reg_strum, customSkin.reg_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_hopo, customSkin.reg_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_tap, customSkin.reg_tap, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_cymbal, customSkin.reg_cymbal, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_strum, customSkin.sp_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_hopo, customSkin.sp_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_tap, customSkin.sp_tap, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_cymbal, customSkin.sp_cymbal, atlusPixels, fullTextureAtlusSize);

        Skin.AssestsAvaliable? sprites = SkinManager.Instance.noteSpritesAvaliable;
        SetCustomTexturesToAtlus(defaultNoteSprites.sustains, customSkin.sustains, atlusPixels, fullTextureAtlusSize);
        SkinManager.Instance.noteSpritesAvaliable = sprites;

        atlus.SetPixels(atlusPixels);
        atlus.Apply();
    }

    void WriteCustomGHLNoteTexturesToAtlus(Texture2D atlus)
    {
        Skin customSkin = SkinManager.Instance.currentSkin;
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
                if (SkinManager.Instance.noteSpritesAvaliable == null)
                    SkinManager.Instance.noteSpritesAvaliable = Skin.AssestsAvaliable.All;
                else if (SkinManager.Instance.noteSpritesAvaliable == Skin.AssestsAvaliable.None)
                    SkinManager.Instance.noteSpritesAvaliable = Skin.AssestsAvaliable.Mixed;

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
                if (SkinManager.Instance.noteSpritesAvaliable == null)
                    SkinManager.Instance.noteSpritesAvaliable = Skin.AssestsAvaliable.None;
                else if (SkinManager.Instance.noteSpritesAvaliable == Skin.AssestsAvaliable.All)
                    SkinManager.Instance.noteSpritesAvaliable = Skin.AssestsAvaliable.Mixed;
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

    void AssignFretSprites()
    {
        Skin customSkin = SkinManager.Instance.currentSkin;

        for (int i = 0; i < customFrets.Length; ++i)
        {
            // Standard Frets
            Sprite sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretBase, null);
            //Sprite sprite = null;
            if (sprite)
            {
                customFrets[i].fretBase = sprite;
                customFrets[i].gameObject.SetActive(true);
            }

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretCover, null);
            if (sprite)
            {
                customFrets[i].fretCover = sprite;
                customFrets[i].gameObject.SetActive(true);
            }

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretPress, null);
            if (sprite)
            {
                customFrets[i].fretPress = sprite;
                customFrets[i].gameObject.SetActive(true);
            }

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretRelease, null);
            if (sprite)
            {
                customFrets[i].fretRelease = sprite;
                customFrets[i].gameObject.SetActive(true);
            }

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretAnim, null);
            if (sprite)
            {
                customFrets[i].toAnimate = sprite;
                customFrets[i].gameObject.SetActive(true);
            }

            // Drum Frets         
            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xDrumFretBase, null);
            if (sprite)
                customFrets[i].drumFretBase = sprite;

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xDrumFretCover, null);
            if (sprite)
                customFrets[i].drumFretCover = sprite;

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xDrumFretPress, null);
            if (sprite)
                customFrets[i].drumFretPress = sprite;

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xDrumFretRelease, null);
            if (sprite)
                customFrets[i].drumFretRelease = sprite;

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xDrumFretAnim, null);
            if (sprite)
                customFrets[i].drumToAnimate = sprite;
        }

        Sprite stem = null;
        Sprite fretStem = customSkin.GetSkinItem<Sprite>(SkinKeys.fretStem, null);
        if (fretStem)
        {
            stem = fretStem;
            for (int i = 0; i < customFrets.Length; ++i)
            {
                customFrets[i].fretStemRen.sprite = stem;
            }
        }
        
        for (int i = 0; i < customFretsGHL.Length; ++i)
        {
            Sprite sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretBaseGhl, null);
            if (sprite)
            {
                customFretsGHL[i].baseRen.sprite = sprite;
                customFretsGHL[i].canUse = true;
            }

            sprite = SkinManager.Instance.GetSkinItem<Sprite>(i + SkinKeys.xFretPressGhl, null);
            if (sprite)
            {
                customFretsGHL[i].pressRen.sprite = sprite;
                customFretsGHL[i].canUse = true;
            }
        }
    }

    void OnApplicationQuit()
    {
        Skin customSkin = SkinManager.Instance.currentSkin;

        // This is purely for the sake of editor resetting, otherwise any custom textures used will be saved between testing
#if UNITY_EDITOR
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
