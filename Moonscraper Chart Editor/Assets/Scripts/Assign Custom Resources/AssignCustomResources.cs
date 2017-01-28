using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public GameplayManager break0;
    public StrikelineAudioController clap;
    public Renderer background;
    public Renderer fretboard;

    Texture initBGTex;
    Texture initFretboardTex;

    // Use this for initialization
    void Start () {
        initBGTex = background.sharedMaterial.mainTexture;
        initFretboardTex = fretboard.sharedMaterial.mainTexture;

        try
        {
            LoadCustomResources resources = GameObject.FindGameObjectWithTag("Resources").GetComponent<LoadCustomResources>();

            if (resources.break0 != null)
                break0.comboBreak = resources.break0;
            if (resources.clap != null)
                clap.clap = resources.clap;
            if (resources.background0 != null)
                background.sharedMaterial.mainTexture = resources.background0;
            if (resources.fretboard != null)
                fretboard.sharedMaterial.mainTexture = resources.fretboard;
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
