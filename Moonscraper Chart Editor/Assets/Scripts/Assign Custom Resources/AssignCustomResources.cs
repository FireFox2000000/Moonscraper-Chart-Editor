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
            
            setSpriteTextures(defaultNoteSprites.reg_strum, customSkin.reg_strum);
            setSpriteTextures(defaultNoteSprites.reg_hopo, customSkin.reg_hopo);
            setSpriteTextures(defaultNoteSprites.reg_tap, customSkin.reg_tap);
            setSpriteTextures(defaultNoteSprites.sp_strum, customSkin.sp_strum);
            setSpriteTextures(defaultNoteSprites.sp_hopo, customSkin.sp_hopo);
            setSpriteTextures(defaultNoteSprites.sp_tap, customSkin.sp_tap);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void setSpriteTextures(Sprite[] sprites, Texture2D[] customTextures)
    {
        for (int i = 0; i < customSkin.reg_strum.Length; ++i)
        {
            if (i < sprites.Length && customTextures[i] && sprites[i]/* && customTextures[i].width == sprites[i].texture.width && customTextures[i].height == sprites[i].texture.height*/)
            {
                Debug.Log(sprites[i].texture.format);
                Debug.Log(customTextures[i].format);
                sprites[i].texture.SetPixels(customTextures[i].GetPixels());
                sprites[i].texture.Apply();
                Debug.Log("PixelsSet");
            }
        }
    }
	
    void OnApplicationQuit()
    {
        // This is purely for the sake of editor resetting, otherwise any custom textures used will be saved between testing
        background.sharedMaterial.mainTexture = initBGTex;
        fretboard.sharedMaterial.mainTexture = initFretboardTex;
    }
}
