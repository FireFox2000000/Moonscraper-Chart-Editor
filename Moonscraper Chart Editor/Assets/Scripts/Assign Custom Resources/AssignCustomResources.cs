using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public GameplayManager break0;
    public StrikelineAudioController clap;
    public Renderer background;
    public Renderer fretboard;
    public Metronome metronome;

    Texture initBGTex;
    Texture initFretboardTex;
    public Skin customSkin;
    public SpriteNoteResources defaultNoteSprites;

    // Use this for initialization
    void Start () {
        initBGTex = background.sharedMaterial.mainTexture;
        initFretboardTex = fretboard.sharedMaterial.mainTexture;

        try
        {
            if (customSkin.break0 != null)
                break0.comboBreak = customSkin.break0;
            if (customSkin.clap != null)
                clap.clap = customSkin.clap;
            if (customSkin.background0 != null)
                background.sharedMaterial.mainTexture = customSkin.background0;
            if (customSkin.fretboard != null)
                fretboard.sharedMaterial.mainTexture = customSkin.fretboard;
            if (customSkin.metronome != null)
                metronome.clap = customSkin.metronome;

            WriteCustomTexturesToAtlus(defaultNoteSprites.fullAtlus);
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

    void WriteCustomTexturesToAtlus(Texture2D atlus)
    {
        Color[] atlusPixels = atlus.GetPixels();
        Utility.IntVector2 fullTextureAtlusSize = new Utility.IntVector2(atlus.width, atlus.height);

        SetCustomTexturesToAtlus(defaultNoteSprites.reg_strum, customSkin.reg_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_hopo, customSkin.reg_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.reg_tap, customSkin.reg_tap, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_strum, customSkin.sp_strum, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_hopo, customSkin.sp_hopo, atlusPixels, fullTextureAtlusSize);
        SetCustomTexturesToAtlus(defaultNoteSprites.sp_tap, customSkin.sp_tap, atlusPixels, fullTextureAtlusSize);

        SetCustomTexturesToAtlus(defaultNoteSprites.sustains, customSkin.sustains, atlusPixels, fullTextureAtlusSize);

        atlus.SetPixels(atlusPixels);
        atlus.Apply();
    }

    void SetCustomTexturesToAtlus(Sprite[] spritesLocation, Texture2D[] customTextures, Color[] fullTextureAtlusPixels, Utility.IntVector2 fullTextureAtlusSize)
    {
        if (spritesLocation.Length != customTextures.Length)
            throw new System.Exception("Mis-aligned sprite locations to textures provided");

        for (int i = 0; i < customTextures.Length; ++i)
        {
            if (customTextures[i] && spritesLocation[i])
            {
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
        }
    }

    void WritePixelsToArea(Color[] texturePixels, Utility.IntVector2 textureSize, Utility.IntVector2 topLeftCornerToStartWriteFrom, Color[] pixelsToOverwrite, Utility.IntVector2 textureToWriteSize)
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
	
    void OnApplicationQuit()
    {
        // This is purely for the sake of editor resetting, otherwise any custom textures used will be saved between testing
        background.sharedMaterial.mainTexture = initBGTex;
        fretboard.sharedMaterial.mainTexture = initFretboardTex;

        // Reset after play
        for (int i = 0; i < customSkin.sustain_colors.Length; ++i)
            customSkin.sustain_colors[i] = new Color(0, 0, 0, 0);
    }
}
