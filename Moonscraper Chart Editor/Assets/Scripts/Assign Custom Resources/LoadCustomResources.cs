using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LoadCustomResources : MonoBehaviour {
    const int NOTE_TEXTURE_1X1_WIDTH = 128, NOTE_TEXTURE_1X1_HEIGHT = 64;
    const int NOTE_TEXTURE_4X2_WIDTH = 256, NOTE_TEXTURE_4X2_HEIGHT = 256;
    const int NOTE_TEXTURE_4X4_WIDTH = 512, NOTE_TEXTURE_4X4_HEIGHT = 256;

    public UnityEngine.UI.Text progressText;
    public ImageFade fader;
    public Skin customSkin;

    static string skinDirectory = "Custom Resources";
    string[] filepaths = new string[0];

    CustomResource[] resources = new CustomResource[] {
        new CustomAudioClip("break-0"),
        new CustomTexture("background-0", 1920, 1080),
        new CustomTexture("fretboard-0", 512, 1024),
        new CustomAudioClip("clap"),
        new CustomAudioClip("metronome"),

        new CustomTexture("0_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
        new CustomTexture("0_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("0_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("0_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),

        new CustomTexture("1_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
        new CustomTexture("1_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("1_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("1_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),

        new CustomTexture("2_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
        new CustomTexture("2_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("2_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("2_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),

        new CustomTexture("3_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
        new CustomTexture("3_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("3_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("3_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),

        new CustomTexture("4_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
        new CustomTexture("4_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("4_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("4_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
    };

    List<CustomResource> resourcesLoading = new List<CustomResource>();

    IEnumerator LoadEditor()
    {
        // Fade
        yield return fader.fadeOut(1.0f);

        // Assign to the custom database
        customSkin.break0 = GetAudioClipFromLoadedResources("break-0", resources);
        customSkin.background0 = GetTextureFromLoadedResources("background-0", resources);
        customSkin.clap = GetAudioClipFromLoadedResources("clap", resources);
        customSkin.fretboard = GetTextureFromLoadedResources("fretboard-0", resources);
        customSkin.metronome = GetAudioClipFromLoadedResources("metronome", resources);

        for (int i = 0; i < customSkin.reg_strum.Length; ++i)
        {
            customSkin.reg_strum[i] = GetTextureFromLoadedResources(i + "_reg_strum", resources);
        }

        for (int i = 0; i < customSkin.reg_hopo.Length; ++i)
        {
            customSkin.reg_hopo[i] = GetTextureFromLoadedResources(i + "_reg_hopo", resources);
        }

        for (int i = 0; i < customSkin.reg_tap.Length; ++i)
        {
            customSkin.reg_tap[i] = GetTextureFromLoadedResources(i + "_reg_tap", resources);
        }

        for (int i = 0; i < customSkin.sp_strum.Length; ++i)
        {
            customSkin.sp_strum[i] = GetTextureFromLoadedResources(i + "_sp_strum", resources);
        }

        for (int i = 0; i < customSkin.sp_hopo.Length; ++i)
        {
            customSkin.sp_hopo[i] = GetTextureFromLoadedResources(i + "_sp_hopo", resources);
        }

        for (int i = 0; i < customSkin.sp_tap.Length; ++i)
        {
            customSkin.sp_tap[i] = GetTextureFromLoadedResources(i + "_sp_tap", resources);
        }

        // Load editor
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        enabled = false;
        fader = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex + 1);
    }

    static Texture2D GetTextureFromLoadedResources(string name, CustomResource[] resources)
    {
        foreach(CustomResource resource in resources)
        {
            if (resource.GetType() == typeof(CustomTexture) && resource.name == name)
            {
                try
                {
                    resource.AssignResource();
                    return ((CustomTexture)resource).texture;
                }
                catch
                {
                    return null;
                }
            }
        }
        return null;
    }

    AudioClip GetAudioClipFromLoadedResources(string name, CustomResource[] resources)
    {
        foreach (CustomResource resource in resources)
        {
            if (resources.GetType() == typeof(CustomAudioClip) && resource.name == name)
            {
                try
                {
                    resource.AssignResource();
                    return ((CustomAudioClip)resource).audio;
                }
                catch
                {
                    return null;
                }
            }
        }
        return null;
    }

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
