// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LoadCustomResources : MonoBehaviour {
    const int FRET_PIXELS_PER_UNIT = 125;

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
    public SustainResources sustainResources;

    static string skinDirectory = "Custom Resources";

    Dictionary<string, CustomResource> resourcesDictionary = new Dictionary<string, CustomResource>();

    List<CustomResource> resources = new List<CustomResource>()
    {
        new CustomAudioClip(SkinKeys.break0),
        new CustomTexture(SkinKeys.fretboard, 512, 1024),
        new CustomAudioClip(SkinKeys.clap),
        new CustomAudioClip(SkinKeys.metronome),

        new CustomTexture("5_reg_strum", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_reg_hopo", OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("5_sp_strum", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),
        new CustomTexture("5_sp_hopo", OPEN_NOTE_TEXTURE_4X4_WIDTH, OPEN_NOTE_TEXTURE_4X4_HEIGHT),

        new CustomTexture("2_reg_strum_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_reg_hopo_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_sp_strum_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),
        new CustomTexture("2_sp_hopo_ghl", GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT),

        new CustomSprite(SkinKeys.fretStem, 64, 16, FRET_PIXELS_PER_UNIT),
        new CustomTexture(SkinKeys.hitFlames, 512, 1024),
    };

    void AddCustomNoteTextureIntoResources()
    {
        // Regular notes
        for (int i = 0; i < 5; ++i)
        {
            resources.AddRange(new CustomTexture[] {
                new CustomTexture(i + "_reg_strum", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_hopo", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_reg_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
                new CustomTexture(i + "_reg_cymbal", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),
                new CustomTexture(i + "_sp_strum", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
                new CustomTexture(i + "_sp_hopo", NOTE_TEXTURE_4X4_WIDTH, NOTE_TEXTURE_4X4_HEIGHT),
                new CustomTexture(i + "_sp_tap", NOTE_TEXTURE_4X2_WIDTH, NOTE_TEXTURE_4X2_HEIGHT),
                new CustomTexture(i + "_sp_cymbal", NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT),

                new CustomSprite(i + SkinKeys.xFretBase, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xFretCover, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xFretPress, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xFretRelease, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xFretAnim, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),

                new CustomSprite(i + SkinKeys.xDrumFretBase, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xDrumFretCover, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xDrumFretPress, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xDrumFretRelease, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xDrumFretAnim, NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT, FRET_PIXELS_PER_UNIT)
            }
            );
        }

        // GHL
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
                new CustomSprite(i + SkinKeys.xFretBaseGhl, GHL_FRET_WIDTH, GHL_FRET_HEIGHT, FRET_PIXELS_PER_UNIT),
                new CustomSprite(i + SkinKeys.xFretPressGhl, GHL_FRET_WIDTH, GHL_FRET_HEIGHT, FRET_PIXELS_PER_UNIT),
            }
            );
        }
    }

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;

#if UNITY_EDITOR
        skinDirectory = Directory.GetParent(Application.dataPath) + "\\" + skinDirectory;
#else
        skinDirectory = DirectoryHelper.GetMainDirectory() + "\\" + skinDirectory;
#endif

        Debug.Log("Loading skin from directory " + skinDirectory);

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
        }
        else
            Debug.LogError("Custom Resources not found");
    }

    List<CustomResource> resourcesLoading = new List<CustomResource>();

    void LoadSettingsConfig(Skin customSkin)
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

            try
            {
                iniparse.Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError("Unable to write to settings inifile stage 1. " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("Encountered unknown exception trying to close settings.ini stage 1. " + e.Message);
            }

            iniparse.Open(skinDirectory + "\\settings.ini");

            for (int i = 0; i < customSkin.sustain_mats.Length; ++i)
            {
                if (customSkin.sustain_mats[i])
                    iniparse.WriteValue("Sustain Colors", i.ToString(), "#" + customSkin.sustain_mats[i].GetColor("_Color").GetHex());
                else
                    iniparse.WriteValue("Sustain Colors", i.ToString(), "#00000000");
            }

            try
            {
                iniparse.Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError("Unable to write to settings inifile stage 2. " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("Encountered unknown exception trying to close settings.ini stage 2. " + e.Message);
            }
        }
    }

    IEnumerator LoadEditor()
    {
        // Fade
        yield return fader.fadeOut(1.0f);
        Skin skin = new Skin();
        LoadSettingsConfig(skin);

        foreach (var skinItem in resourcesDictionary)
        {
            skinItem.Value.AssignResource();

            // Add all loaded custom assets into the skin manager. Probably move this whole loading function into there later?
            skin.AddSkinItem(skinItem.Key, skinItem.Value.filepath, skinItem.Value.GetObject());  
        }      

        // STANDARD NOTES
        for (int i = 0; i < skin.reg_strum.Length; ++i)
        {
            skin.reg_strum[i] = GetTextureFromLoadedResources(i + "_reg_strum", resourcesDictionary);
        }

        for (int i = 0; i < skin.reg_hopo.Length; ++i)
        {
            skin.reg_hopo[i] = GetTextureFromLoadedResources(i + "_reg_hopo", resourcesDictionary);
        }

        for (int i = 0; i < skin.reg_tap.Length; ++i)
        {
            skin.reg_tap[i] = GetTextureFromLoadedResources(i + "_reg_tap", resourcesDictionary);
        }

        for (int i = 0; i < skin.reg_cymbal.Length; ++i)
        {
            skin.reg_cymbal[i] = GetTextureFromLoadedResources(i + "_reg_cymbal", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_strum.Length; ++i)
        {
            skin.sp_strum[i] = GetTextureFromLoadedResources(i + "_sp_strum", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_hopo.Length; ++i)
        {
            skin.sp_hopo[i] = GetTextureFromLoadedResources(i + "_sp_hopo", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_tap.Length; ++i)
        {
            skin.sp_tap[i] = GetTextureFromLoadedResources(i + "_sp_tap", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_cymbal.Length; ++i)
        {
            skin.sp_cymbal[i] = GetTextureFromLoadedResources(i + "_sp_cymbal", resourcesDictionary);
        }

        // GHL LOADING
        for (int i = 0; i < skin.reg_strum_ghl.Length; ++i)
        {
            skin.reg_strum_ghl[i] = GetTextureFromLoadedResources(i + "_reg_strum_ghl", resourcesDictionary);
        }

        for (int i = 0; i < skin.reg_hopo_ghl.Length; ++i)
        {
            skin.reg_hopo_ghl[i] = GetTextureFromLoadedResources(i + "_reg_hopo_ghl", resourcesDictionary);
        }

        for (int i = 0; i < skin.reg_tap_ghl.Length; ++i)
        {
            skin.reg_tap_ghl[i] = GetTextureFromLoadedResources(i + "_reg_tap_ghl", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_strum_ghl.Length; ++i)
        {
            skin.sp_strum_ghl[i] = GetTextureFromLoadedResources(i + "_sp_strum_ghl", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_hopo_ghl.Length; ++i)
        {
            skin.sp_hopo_ghl[i] = GetTextureFromLoadedResources(i + "_sp_hopo_ghl", resourcesDictionary);
        }

        for (int i = 0; i < skin.sp_tap_ghl.Length; ++i)
        {
            skin.sp_tap_ghl[i] = GetTextureFromLoadedResources(i + "_sp_tap_ghl", resourcesDictionary);
        }

        SkinManager.Instance.currentSkin = skin;

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
            return resource.GetObject() as Texture2D;
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
            return resource.GetObject() as AudioClip;
        }
        catch
        {
            return null;
        }
    }
}
