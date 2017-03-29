using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LoadCustomResources : MonoBehaviour {
    public UnityEngine.UI.Text progressText;
    public ImageFade fader;
    public Skin customSkin;

    static string skinDirectory = "Custom Resources";
    string[] filepaths = new string[0];

    CustomResource[] resources = new CustomResource[] {
        new CustomAudioClip("break-0"),
        new CustomTexture("background-0", 1920, 1080),
        new CustomTexture("fretboard-0", 512, 1024),
        new CustomAudioClip("clap")
    };

    public AudioClip break0 { get { return ((CustomAudioClip)resources[0]).audio; } }
    public Texture2D background0 { get { return ((CustomTexture)resources[1]).texture; } }
    public Texture2D fretboard { get { return ((CustomTexture)resources[2]).texture; } }
    public AudioClip clap { get { return ((CustomAudioClip)resources[3]).audio; } }

    List<CustomResource> resourcesLoading = new List<CustomResource>();

    // Use this for initialization
    void Start () {
        if (Directory.Exists(skinDirectory))
        {
            // Collect all the files
            filepaths = GetAllFiles(skinDirectory).ToArray();

            foreach (CustomResource resource in resources)
            {
                if (resource.InitWWW(filepaths))
                {
                    resourcesLoading.Add(resource);
                }
            }
        }
        else
            Debug.LogError("Custom Resources not found");
	}
	
	// Update is called once per frame
	void Update () {
        float progress = 0;
        bool complete = true;

        // Total all www load processes
        foreach(CustomResource resource in resourcesLoading)
        {
            progress += resource.www.progress;
            if (!resource.www.isDone)
                complete = false;
        }

        // Update progress bar
        if (resourcesLoading.Count > 0)
            progress /= resourcesLoading.Count;
        else
            progress = 1;

        progressText.text = "Loading custom resources... " + Mathf.Round(progress * 100).ToString() + "%";

        // Wait until all wwws are fully loaded before editing the custom skin
        if (complete && !fader.fadeOutRunning)
        {
            StartCoroutine(LoadEditor());
        }
    }

    IEnumerator LoadEditor()
    {
        // Fade
        yield return fader.fadeOut(1.0f);

        // Link the loaded asset to the public file we can grab from
        foreach (CustomResource resource in resourcesLoading)
        {
            resource.AssignResource();
        }

        // Assign to the custom database
        customSkin.break0 = break0;
        customSkin.background0 = background0;
        customSkin.clap = clap;
        customSkin.fretboard = fretboard;

        // Load editor
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        enabled = false;
        fader = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex + 1);
    }

    List<string> GetAllFiles(string dir)
    {
        List<string> files = new List<string>();
        try
        {
            foreach (string f in Directory.GetFiles(dir))
            {
                files.Add(f);
            }
            foreach (string d in Directory.GetDirectories(dir))
            {
                files.AddRange(GetAllFiles(d));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        return files;
    }
}
