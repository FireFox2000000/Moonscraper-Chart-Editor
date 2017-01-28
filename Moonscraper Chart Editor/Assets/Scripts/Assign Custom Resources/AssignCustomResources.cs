using UnityEngine;
using System.Collections;

public class AssignCustomResources : MonoBehaviour {
    public GameplayManager break0;
    public StrikelineAudioController clap;
    public Renderer background;

    Texture initTex;

	// Use this for initialization
	void Start () {
        LoadCustomResources resources = GameObject.FindGameObjectWithTag("Resources").GetComponent<LoadCustomResources>();

        initTex = background.sharedMaterial.mainTexture;

        if (resources.break0 != null)
            break0.comboBreak = resources.break0;
        if (resources.clap != null)
            clap.clap = resources.clap;
        if (resources.background0 != null)
            background.sharedMaterial.mainTexture = resources.background0;
    }
	
    void OnApplicationQuit()
    {
        background.sharedMaterial.mainTexture = initTex;
    }
}
