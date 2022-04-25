// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using DaVikingCode.AssetPacker;

[RequireComponent(typeof(AssetPacker))]
public class LoadCustomResources : MonoBehaviour {
    const int FRET_PIXELS_PER_UNIT = 125;

    const int NOTE_TEXTURE_1X1_WIDTH = 128, NOTE_TEXTURE_1X1_HEIGHT = 64;

    const int OPEN_NOTE_TEXTURE_1X1_WIDTH = 512, OPEN_NOTE_TEXTURE_1X1_HEIGHT = 64;

    const int GHL_NOTE_TEXTURE_1X1_WIDTH = 100,         GHL_NOTE_TEXTURE_1X1_HEIGHT = 100;
    const int GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH = 512,    GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT = 64;

    const int GHL_FRET_WIDTH = 100, GHL_FRET_HEIGHT = 100;

    const int SUSTAIN_TEXTURE_WIDTH = 32, SUSTAIN_TEXTURE_HEIGHT = 32;

    public UnityEngine.UI.Text progressText;
    public ImageFade fader;
    public SustainResources sustainResources;
    AssetPacker assetPacker;

    static string skinDirectory = "Custom Resources";

    Dictionary<string, CustomResource> resourcesDictionary = new Dictionary<string, CustomResource>();

    List<CustomResource> resources = new List<CustomResource>()
    {
        new CustomAudioClip(SkinKeys.break0),
        new CustomAudioClip(SkinKeys.clap),
        new CustomAudioClip(SkinKeys.metronome),

        // Any images we don't want added to the sprite atlus
        new CustomTexture(SkinKeys.fretboard, 512, 1024),
        new CustomTexture(SkinKeys.hitFlames, 512, 1024),
    };

    // Textures we want to pack into the sprite atlus
    Dictionary<string, TextureToPack.GridSlice> imagesToPack = new Dictionary<string, TextureToPack.GridSlice>()    // Prefix/size pair
    {
        // open
        { "5_reg_hopo",     new TextureToPack.GridSlice(OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "5_reg_strum",    new TextureToPack.GridSlice(OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "5_sp_hopo",      new TextureToPack.GridSlice(OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "5_sp_strum",     new TextureToPack.GridSlice(OPEN_NOTE_TEXTURE_1X1_WIDTH, OPEN_NOTE_TEXTURE_1X1_HEIGHT) },

        { "2_reg_strum_ghl",        new TextureToPack.GridSlice(GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "2_reg_hopo_ghl",         new TextureToPack.GridSlice(GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "2_sp_strum_ghl",         new TextureToPack.GridSlice(GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT) },
        { "2_sp_hopo_ghl",          new TextureToPack.GridSlice(GHL_OPEN_NOTE_TEXTURE_1X1_WIDTH, GHL_OPEN_NOTE_TEXTURE_1X1_HEIGHT) },

        { SkinKeys.fretStem, null },
        { SkinKeys.measureBeatLine, null },
        { SkinKeys.standardBeatLine, null },
        { SkinKeys.quarterBeatLine, null },
    };

    void SetupResourcesToLoad()
    {
        // Textures we want to pack into the atlus, open notes already added
        {
            for (int i = 0; i < 5; ++i)
            {
                imagesToPack.Add(i + "_reg_strum", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_hopo", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_tap", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_pad", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_cymbal", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));

                imagesToPack.Add(i + "_sp_strum", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_hopo", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_tap", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_pad", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_cymbal", new TextureToPack.GridSlice(NOTE_TEXTURE_1X1_WIDTH, NOTE_TEXTURE_1X1_HEIGHT));

                imagesToPack.Add(i + SkinKeys.xFretBase, null);
                imagesToPack.Add(i + SkinKeys.xFretCover, null);
                imagesToPack.Add(i + SkinKeys.xFretPress, null);
                imagesToPack.Add(i + SkinKeys.xFretRelease, null);
                imagesToPack.Add(i + SkinKeys.xFretAnim, null);

                imagesToPack.Add(i + SkinKeys.xDrumFretBase, null);
                imagesToPack.Add(i + SkinKeys.xDrumFretCover, null);
                imagesToPack.Add(i + SkinKeys.xDrumFretPress, null);
                imagesToPack.Add(i + SkinKeys.xDrumFretRelease, null);
                imagesToPack.Add(i + SkinKeys.xDrumFretAnim, null);
            }

            // GHL
            for (int i = 0; i < 2; ++i)
            {
                imagesToPack.Add(i + "_reg_strum_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_hopo_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_reg_tap_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_strum_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_hopo_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
                imagesToPack.Add(i + "_sp_tap_ghl", new TextureToPack.GridSlice(GHL_NOTE_TEXTURE_1X1_WIDTH, GHL_NOTE_TEXTURE_1X1_HEIGHT));
            }

            for (int i = 0; i < 6; ++i)
            {
                imagesToPack.Add(i + SkinKeys.xFretBaseGhl, null);
                imagesToPack.Add(i + SkinKeys.xFretPressGhl, null);
            }
        }

        // Any images we're packing into the sprite sheet need to loaded as textures via our resources. Automatically add these to the list.
        foreach (var image in imagesToPack)
        {
            resources.Add(new CustomTexture(image.Key));
        }
    }

    delegate void UpdateFn();
    UpdateFn currentState;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        assetPacker = GetComponent<AssetPacker>();

        currentState = UpdateWaitingForResourcesLoaded;

#if UNITY_EDITOR
        skinDirectory = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), skinDirectory);
#else
        skinDirectory = Path.Combine(DirectoryHelper.GetMainDirectory(), skinDirectory);
#endif

        Debug.Log("Loading skin from directory " + skinDirectory);

        if (Directory.Exists(skinDirectory))
        {
            SetupResourcesToLoad();

            // Collect all the files
            string[] filepaths = GetAllFiles(skinDirectory).ToArray();
            Dictionary<string, string> filepathsDictionary = new Dictionary<string, string>();

            int bgCount = 0;

            foreach (string path in filepaths)
            {
                string assetKey = Path.GetFileNameWithoutExtension(path);
                if (!filepathsDictionary.ContainsKey(assetKey))
                {
                    filepathsDictionary.Add(assetKey, path);
                }
                else
                {
                    Debug.LogWarning("Found a duplicate custom asset with under a different file extenstion. Ignoring asset " + path);
                    continue;
                }

                // Checking if the file provided is a background. We have no limit on the amount of backgrounds we can load, so we can't pre-define them like we do above.
                if (System.Text.RegularExpressions.Regex.Match(assetKey, @"background-[0-9]+").Success)
                {
                    resources.Add(new CustomTexture(assetKey, 1920, 1080));
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
	
	// Update is called once per frame
	void Update ()
    {
        currentState();
    }

    void UpdateWaitingForResourcesLoaded()
    {
        float progress = 0;
        bool complete = true;

        foreach (CustomResource resource in resourcesLoading)
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

        if (complete)
        {
            OnResourceLoadingComplete();
            currentState = UpdateWaitingForPackingCompelte;
        }
    }

    void UpdateWaitingForPackingCompelte()
    {
        // Empty on purpose. We don't need to do anything except wait for texture packing to finish. We already have a listener set up to change this state on the asset packer.
    }

    void OnResourceLoadingComplete()
    {
        foreach (var imageInfo in imagesToPack)
        {
            Texture2D texture = GetTextureFromLoadedResources(imageInfo.Key, resourcesDictionary);
            if (texture)
                assetPacker.AddTextureToPack(texture, imageInfo.Value, imageInfo.Key);
        }

        progressText.text = "Packing Textures...";

        assetPacker.OnProcessCompleted.AddListener(OnTexturePackingComplete);
        assetPacker.Process();
    }

    void OnTexturePackingComplete()
    {
        // Transfer sprites over to skin to hold
        Dictionary<string, Sprite[]> packedSprites = new Dictionary<string, Sprite[]>();

        foreach (var imageInfo in imagesToPack)
        {
            Sprite[] sprites = assetPacker.GetSprites(imageInfo.Key);
            if (sprites.Length > 0)
            {
                // Asset packer returns sprites via prefix. GHL specific assets have a "_ghl" suffix. Will cause ghl assets to get mixed with normal ones. Need to fix.
                {
                    const string GHL_ID = "_ghl";
                    if (!imageInfo.Key.Contains(GHL_ID))
                    {
                        List<Sprite> spriteList = new List<Sprite>();
                        spriteList.AddRange(sprites);

                        for (int i = spriteList.Count - 1; i >= 0; --i)
                        {
                            if (spriteList[i].name.Contains(GHL_ID))
                            {
                                spriteList.RemoveAt(i);
                            }
                        }

                        sprites = spriteList.ToArray();
                    }
                }

                packedSprites.Add(imageInfo.Key, sprites);
            }
        }

        progressText.text = "Loading Complete!";

        StartCoroutine(LoadEditor(packedSprites));
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

    IEnumerator LoadEditor(Dictionary<string, Sprite[]> packedSprites)
    {
        // Fade
        yield return fader.fadeOut(1.0f);
        Skin skin = new Skin();
        LoadSettingsConfig(skin);

        skin.SetSpriteSheet(packedSprites);

        foreach (var skinItem in resourcesDictionary)
        {
            if (imagesToPack.ContainsKey(skinItem.Key))
                continue;

            skinItem.Value.AssignResource();

            // Add all loaded custom assets into the skin manager. Probably move this whole loading function into there later?
            skin.AddSkinItem(skinItem.Key, skinItem.Value.filepath, skinItem.Value.GetObject());
        }

        SkinManager.Instance.currentSkin = skin;

        // Load editor
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        enabled = false;
        fader = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex + 1);
    }

    void LoadSettingsConfig(Skin customSkin)
    {
        if (Directory.Exists(skinDirectory))
        {
            // Load in all settings
            INIParser iniparse = new INIParser();

            iniparse.Open(Path.Combine(skinDirectory, "settings.ini"));
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

            iniparse.Open(Path.Combine(skinDirectory, "settings.ini"));

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
}
