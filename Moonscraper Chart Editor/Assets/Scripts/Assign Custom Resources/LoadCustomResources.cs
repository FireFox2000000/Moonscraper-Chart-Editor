using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LoadCustomResources : MonoBehaviour {
    const int NOTE_TEXTURE_1X1_WIDTH = 128, NOTE_TEXTURE_1X1_HEIGHT = 64;
    const int NOTE_TEXTURE_4X2_WIDTH = 256, NOTE_TEXTURE_4X2_HEIGHT = 256;
    const int NOTE_TEXTURE_4X4_WIDTH = 512, NOTE_TEXTURE_4X4_HEIGHT = 256;

    const int OPEN_NOTE_TEXTURE_1X1_WIDTH = 512, OPEN_NOTE_TEXTURE_1X1_HEIGHT = 64;
    const int OPEN_NOTE_TEXTURE_4X4_WIDTH = 2048, OPEN_NOTE_TEXTURE_4X4_HEIGHT = 256;

    const int SUSTAIN_TEXTURE_WIDTH = 32, SUSTAIN_TEXTURE_HEIGHT = 32;

    public UnityEngine.UI.Text progressText;
    public ImageFade fader;
    public Skin customSkin;
    public SustainResources sustainResources;

    static string skinDirectory = "Custom Resources";
    string[] filepaths = new string[0];

    Dictionary<string, CustomResource> resourcesDictionary = new Dictionary<string, CustomResource>();

    CustomResource[] resources = new CustomResource[] 
    {
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

        new CustomTexture("5_reg_strum", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_reg_hopo", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_sp_strum", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("5_sp_hopo", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),
        /*
        new CustomTexture("0_sustain", SUSTAIN_TEXTURE_WIDTH, SUSTAIN_TEXTURE_HEIGHT),
        new CustomTexture("1_sustain", SUSTAIN_TEXTURE_WIDTH, SUSTAIN_TEXTURE_HEIGHT),
        new CustomTexture("2_sustain", SUSTAIN_TEXTURE_WIDTH, SUSTAIN_TEXTURE_HEIGHT),
        new CustomTexture("3_sustain", SUSTAIN_TEXTURE_WIDTH, SUSTAIN_TEXTURE_HEIGHT),
        new CustomTexture("4_sustain", SUSTAIN_TEXTURE_WIDTH, SUSTAIN_TEXTURE_HEIGHT),*/

        new CustomTexture("0_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("0_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("1_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("1_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("2_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("3_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("3_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("4_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("4_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("fret_stem", 64, 16),
        new CustomTexture("hit_flames", 512, 1024),
    };

    List<CustomResource> resourcesLoading = new List<CustomResource>();

    void LoadSettingsConfig()
    {
        if (Directory.Exists(skinDirectory))
        {
            // Load in all settings
            INIParser iniparse = new INIParser();

            iniparse.Open(skinDirectory + "\\settings.ini");
            System.Text.RegularExpressions.Regex hexRegex = new System.Text.RegularExpressions.Regex("#[a-fA-F0-9]{8,8}");

            for (int i = 0; i < customSkin.sustain_mats.Length; ++i)
            {
                string hex = iniparse.ReadValue("Sustain Colors", i.ToString(), "#00000000");
                if (hex.Length == 9 && hexRegex.IsMatch(hex))    // # r g b a
                {
                    try
                    {
                        int r = int.Parse(new string(new char[] { hex[1], hex[2] }), System.Globalization.NumberStyles.HexNumber);
                        int g = int.Parse(new string(new char[] { hex[3], hex[4] }), System.Globalization.NumberStyles.HexNumber);
                        int b = int.Parse(new string(new char[] { hex[5], hex[6] }), System.Globalization.NumberStyles.HexNumber);
                        int a = int.Parse(new string(new char[] { hex[7], hex[8] }), System.Globalization.NumberStyles.HexNumber);

                        if (a > 0)
                        {
                            customSkin.sustain_mats[i] = new Material(sustainResources.sustainColours[i]);
                            customSkin.sustain_mats[i].name = i.ToString();
                            customSkin.sustain_mats[i].SetColor("_Color", new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }

                //iniparse.WriteValue("Sustain Colors", i.ToString(), customSkin.sustain_colors[i].GetHex());
            }

            iniparse.Close();

            iniparse.Open(skinDirectory + "\\settings.ini");

            for (int i = 0; i < customSkin.sustain_mats.Length; ++i)
            {
                if (customSkin.sustain_mats[i])
                    iniparse.WriteValue("Sustain Colors", i.ToString(), "#" + customSkin.sustain_mats[i].GetColor("_Color").GetHex());
                else
                    iniparse.WriteValue("Sustain Colors", i.ToString(), "#00000000");
            }

            iniparse.Close();
        }
    }

    IEnumerator LoadEditor()
    {
        // Fade
        yield return fader.fadeOut(1.0f);

        // Assign to the custom database
        customSkin.break0 = GetAudioClipFromLoadedResources("break-0", resourcesDictionary);
        customSkin.background0 = GetTextureFromLoadedResources("background-0", resourcesDictionary);
        customSkin.clap = GetAudioClipFromLoadedResources("clap", resourcesDictionary);
        customSkin.fretboard = GetTextureFromLoadedResources("fretboard-0", resourcesDictionary);
        customSkin.metronome = GetAudioClipFromLoadedResources("metronome", resourcesDictionary);

        for (int i = 0; i < customSkin.reg_strum.Length; ++i)
        {
            customSkin.reg_strum[i] = GetTextureFromLoadedResources(i + "_reg_strum", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.reg_hopo.Length; ++i)
        {
            customSkin.reg_hopo[i] = GetTextureFromLoadedResources(i + "_reg_hopo", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.reg_tap.Length; ++i)
        {
            customSkin.reg_tap[i] = GetTextureFromLoadedResources(i + "_reg_tap", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_strum.Length; ++i)
        {
            customSkin.sp_strum[i] = GetTextureFromLoadedResources(i + "_sp_strum", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_hopo.Length; ++i)
        {
            customSkin.sp_hopo[i] = GetTextureFromLoadedResources(i + "_sp_hopo", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_tap.Length; ++i)
        {
            customSkin.sp_tap[i] = GetTextureFromLoadedResources(i + "_sp_tap", resourcesDictionary);
        }
        /*
        for (int i = 0; i < customSkin.sustains.Length; ++i)
        {
            customSkin.sustains[i] = GetTextureFromLoadedResources(i + "_sustain", resourcesDictionary);
        }*/

        for (int i = 0; i < customSkin.fret_base.Length; ++i)
        {
            customSkin.fret_base[i] = GetTextureFromLoadedResources(i + "_fret_base", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_cover.Length; ++i)
        {
            customSkin.fret_cover[i] = GetTextureFromLoadedResources(i + "_fret_cover", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_press.Length; ++i)
        {
            customSkin.fret_press[i] = GetTextureFromLoadedResources(i + "_fret_press", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_release.Length; ++i)
        {
            customSkin.fret_release[i] = GetTextureFromLoadedResources(i + "_fret_release", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_anim.Length; ++i)
        {
            customSkin.fret_anim[i] = GetTextureFromLoadedResources(i + "_fret_anim", resourcesDictionary);
        }

        customSkin.fret_stem = GetTextureFromLoadedResources("fret_stem", resourcesDictionary);
        customSkin.hit_flames = GetTextureFromLoadedResources("hit_flames", resourcesDictionary);

        // Load editor
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        enabled = false;
        fader = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex + 1);
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
                    resourcesDictionary.Add(resource.name, resource);
                }
            }

            LoadSettingsConfig();
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

    static Texture2D GetTextureFromLoadedResources(string name, Dictionary<string, CustomResource> resources)
    {
        CustomResource resource;
        resources.TryGetValue(name, out resource);

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

    static AudioClip GetAudioClipFromLoadedResources(string name, Dictionary<string, CustomResource> resources)
    {
        CustomResource resource;
        resources.TryGetValue(name, out resource);
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
