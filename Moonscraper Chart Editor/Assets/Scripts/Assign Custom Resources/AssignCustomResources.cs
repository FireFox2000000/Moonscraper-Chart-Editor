using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public GameplayManager break0;
    public StrikelineAudioController clap;
    public Renderer background;
    public Renderer fretboard;

    Texture initBGTex;
    Texture initFretboardTex;
    public Skin currentSkin;

    // Use this for initialization
    void Start () {
        initBGTex = background.sharedMaterial.mainTexture;
        initFretboardTex = fretboard.sharedMaterial.mainTexture;

        try
        {
            if (currentSkin.break0 != null)
                break0.comboBreak = currentSkin.break0;
            if (currentSkin.clap != null)
                clap.clap = currentSkin.clap;
            if (currentSkin.background0 != null)
                background.sharedMaterial.mainTexture = currentSkin.background0;
            if (currentSkin.fretboard != null)
                fretboard.sharedMaterial.mainTexture = currentSkin.fretboard;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
	
    void OnApplicationQuit()
    {
        background.sharedMaterial.mainTexture = initBGTex;
        fretboard.sharedMaterial.mainTexture = initFretboardTex;
    }
}
