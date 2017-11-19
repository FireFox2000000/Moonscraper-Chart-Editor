// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

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

    const int GHL_NOTE_TEXTURE_1X1_WIDTH = 100,         GHL_NOTE_TEXTURE_1X1_HEIGHT = 100;
    const int GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH = 400,    GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT = 50;

    const int GHL_FRET_WIDTH = 100, GHL_FRET_HEIGHT = 100;

    const int SUSTAIN_TEXTURE_WIDTH = 32, SUSTAIN_TEXTURE_HEIGHT = 32;

    public UnityEngine.UI.Text progressText;
    public ImageFade fader;
    public Skin customSkin;
    public SustainResources sustainResources;

    static string skinDirectory = "Custom Resources";

    Dictionary<string, CustomResource> resourcesDictionary = new Dictionary<string, CustomResource>();

    List<CustomResource> resources = new List<CustomResource>()
    {
        new CustomAudioClip("break-0"),
        //new CustomTexture("background-0", 1920, 1080),
        new CustomTexture("fretboard-0", 512, 1024),
        new CustomAudioClip("clap"),
        new CustomAudioClip("metronome"),

        new CustomTexture("5_reg_strum", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_reg_hopo", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_sp_strum", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("5_sp_hopo", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),

        new CustomTexture("2_reg_strum_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_reg_hopo_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_sp_strum_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_sp_hopo_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),

        new CustomTexture("fret_stem", 64, 16),
        new CustomTexture("hit_flames", 512, 1024),
    };

    void AddCustomNoteTextureIntoResources()
    {
        for (int i = 0; i < 5; ++i)
        {
            resources.AddRange(new CustomTexture[] {
                new CustomTexture(i + "_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
                new CustomTexture(i + "_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
                new CustomTexture(i + "_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
                new CustomTexture(i + "_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),

                new CustomTexture(i + "_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

                new CustomTexture(i + "_drum_fret_base", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_drum_fret_cover", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_drum_fret_press", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_drum_fret_release", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_drum_fret_anim", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT)
            }
            );
        }

        for (int i = 0; i < 2; ++i)
        {
            resources.AddRange(new CustomTexture[] {
                new CustomTexture(i + "_reg_strum_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_hopo_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_tap_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_sp_strum_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_sp_hopo_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_sp_tap_ghl", GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT),
            }
            );
        }

        for (int i = 0; i < 6; ++i)
        {
            resources.AddRange(new CustomTexture[] {
                new CustomTexture(i + "_fret_base_ghl", GHL_FRET_WIDTH, GHL_FRET_HEIGHT),
                new CustomTexture(i + "_fret_press_ghl", GHL_FRET_WIDTH, GHL_FRET_HEIGHT),
            }
            );
        }
    }

    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR
        skinDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + skinDirectory;
#endif
        if (Directory.Exists(skinDirectory))
        {
            AddCustomNoteTextureIntoResources();

            // Collect all the files
            string[] filepaths = GetAllFiles(skinDirectory).ToArray();
            Dictionary<string, string> filepathsDictionary = new Dictionary<string, string>();

            int bgCount = 0;

            foreach (string path in filepaths)
            {
                filepathsDictionary.Add(Path.GetFileNameWithoutExtension(path), path);

                // System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(path), @"background-/([0-9]+)$");
                //if ( Path.GetFileNameWithoutExtension(path).Contains("background-\d+"))
                if (System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(path), @"background-[0-9]+").Success)
                {
                    resources.Add(new CustomTexture(Path.GetFileNameWithoutExtension(path), 1920, 1080));
                    ++bgCount;
                }
            }

            Debug.Log("Total backgrounds: " + bgCount);

            foreach (CustomResource resource in resources)
            {
                if (resource.InitWWW(filepathsDictionary))
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

        int bgCount = 0;
        Texture2D tex = null;
        List<Texture2D> textures = new List<Texture2D>();
        while (true)
        {
            tex = GetTextureFromLoadedResources("background-" + bgCount++, resourcesDictionary);

            if (!tex)
                break;
            textures.Add(tex);
        }
        customSkin.backgrounds = textures.ToArray();

        customSkin.clap = GetAudioClipFromLoadedResources("clap", resourcesDictionary);
        customSkin.fretboard = GetTextureFromLoadedResources("fretboard-0", resourcesDictionary);
        customSkin.metronome = GetAudioClipFromLoadedResources("metronome", resourcesDictionary);

        // STANDARD NOTES
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

        // STANDARD FRETS
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
        
        // DRUMS
        for (int i = 0; i < customSkin.fret_base.Length; ++i)
        {
            customSkin.drum_fret_base[i] = GetTextureFromLoadedResources(i + "_drum_fret_base", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_cover.Length; ++i)
        {
            customSkin.drum_fret_cover[i] = GetTextureFromLoadedResources(i + "_drum_fret_cover", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_press.Length; ++i)
        {
            customSkin.drum_fret_press[i] = GetTextureFromLoadedResources(i + "_drum_fret_press", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_release.Length; ++i)
        {
            customSkin.drum_fret_release[i] = GetTextureFromLoadedResources(i + "_drum_fret_release", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_anim.Length; ++i)
        {
            customSkin.drum_fret_anim[i] = GetTextureFromLoadedResources(i + "_drum_fret_anim", resourcesDictionary);
        }

        // GHL LOADING
        for (int i = 0; i < customSkin.reg_strum_ghl.Length; ++i)
        {
            customSkin.reg_strum_ghl[i] = GetTextureFromLoadedResources(i + "_reg_strum_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.reg_hopo_ghl.Length; ++i)
        {
            customSkin.reg_hopo_ghl[i] = GetTextureFromLoadedResources(i + "_reg_hopo_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.reg_tap_ghl.Length; ++i)
        {
            customSkin.reg_tap_ghl[i] = GetTextureFromLoadedResources(i + "_reg_tap_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_strum_ghl.Length; ++i)
        {
            customSkin.sp_strum_ghl[i] = GetTextureFromLoadedResources(i + "_sp_strum_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_hopo_ghl.Length; ++i)
        {
            customSkin.sp_hopo_ghl[i] = GetTextureFromLoadedResources(i + "_sp_hopo_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.sp_tap_ghl.Length; ++i)
        {
            customSkin.sp_tap_ghl[i] = GetTextureFromLoadedResources(i + "_sp_tap_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_base_ghl.Length; ++i)
        {
            customSkin.fret_base_ghl[i] = GetTextureFromLoadedResources(i + "_fret_base_ghl", resourcesDictionary);
        }

        for (int i = 0; i < customSkin.fret_press_ghl.Length; ++i)
        {
            customSkin.fret_press_ghl[i] = GetTextureFromLoadedResources(i + "_fret_press_ghl", resourcesDictionary);
        }

        customSkin.fret_stem = GetTextureFromLoadedResources("fret_stem", resourcesDictionary);
        customSkin.hit_flames = GetTextureFromLoadedResources("hit_flames", resourcesDictionary);

        // Load editor
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        enabled = false;
        fader = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex + 1);
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
