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
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
	
    void OnApplicationQuit()
    {
        // This is purely for the sake of editor resetting, otherwise any custom textures used will be saved between testing
        background.sharedMaterial.mainTexture = initBGTex;
        fretboard.sharedMaterial.mainTexture = initFretboardTex;
    }
}
