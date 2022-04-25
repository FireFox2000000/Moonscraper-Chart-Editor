// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using System;

public class GameSettings
{
    const int VersionNumber = 1;
    const string SECTION_NAME_METADATA = "MetaData";
    const string SECTION_NAME_SETTINGS = "Settings";
    const string SECTION_NAME_AUDIO = "Audio Volume";
    const string SECTION_NAME_GRAPHICS = "Graphics";
    const string SECTION_NAME_LYRICEDITOR = "Lyric Editor";
    const string SECTION_NAME_SONG = "Song";

    public class LyricEditorSettings
    {
        public bool stepSnappingEnabled = true;
        public float phaseEndThreashold = 0f;
    }

    [Flags]
    public enum ClapToggle
    {
        NONE = 0,

        STRUM = 1 << 0,
        HOPO = 1 << 1,
        TAP = 1 << 2,

        STARPOWER = 1 << 3,
        CHARTEVENT = 1 << 4,

        BPM = 1 << 5,
        TS = 1 << 6,
        EVENT = 1 << 7,
        SECTION = 1 << 8,

        ALL_NOTES = 1 << 30
    }

    public enum NotePlacementMode
    {
        Default, LeftyFlip
    }

    public enum DrumModeOptions
    {
        Standard,
        ProDrums,
    }

    public abstract class SaveSetting
    {
        public string categoryKey;
        public string saveKey;

        public SaveSetting(string categoryKey, string saveKey)
        {
            this.categoryKey = categoryKey;
            this.saveKey = saveKey;
        }

        public abstract void WriteToIni(INIParser iniparse);
        public abstract void ReadFromIni(INIParser iniparse);
        public abstract override string ToString();
    }

    public class IntSaveSetting : SaveSetting
    {
        public int value;
        public static implicit operator int(IntSaveSetting v) { return v.value; }

        public int defaultValue;
        public IntSaveSetting(string categoryKey, string saveKey, int defaultVal) : base(categoryKey, saveKey)
        {
            defaultValue = defaultVal;
            value = defaultValue;
        }

        public override void WriteToIni(INIParser iniparse)
        {
            iniparse.WriteValue(categoryKey, saveKey, value);
        }

        public override void ReadFromIni(INIParser iniparse)
        {
            value = iniparse.ReadValue(categoryKey, saveKey, defaultValue);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class FloatSaveSetting : SaveSetting
    {
        public float value;      
        public static implicit operator float(FloatSaveSetting v) { return v.value; }

        public float defaultValue;
        public FloatSaveSetting(string categoryKey, string saveKey, float defaultVal) : base(categoryKey, saveKey)
        {
            defaultValue = defaultVal;
            value = defaultValue;
        }

        public override void WriteToIni(INIParser iniparse)
        {
            iniparse.WriteValue(categoryKey, saveKey, value);
        }

        public override void ReadFromIni(INIParser iniparse)
        {
            value = (float)iniparse.ReadValue(categoryKey, saveKey, defaultValue);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class BoolSaveSetting : SaveSetting
    {
        public bool value;
        public static implicit operator bool(BoolSaveSetting v) { return v.value; }

        public bool defaultValue;
        public BoolSaveSetting(string categoryKey, string saveKey, bool defaultVal) : base(categoryKey, saveKey)
        {
            defaultValue = defaultVal;
            value = defaultValue;
        }

        public override void WriteToIni(INIParser iniparse)
        {
            iniparse.WriteValue(categoryKey, saveKey, value);
        }

        public override void ReadFromIni(INIParser iniparse)
        {
            value = iniparse.ReadValue(categoryKey, saveKey, defaultValue);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class EnumSaveSetting<EnumType> : SaveSetting where EnumType : Enum
    {
        public EnumType value;
        public static implicit operator EnumType(EnumSaveSetting<EnumType> v) { return v.value; }

        public EnumType defaultValue;
        public EnumSaveSetting(string categoryKey, string saveKey, EnumType defaultVal) : base(categoryKey, saveKey)
        {
            defaultValue = defaultVal;
            value = defaultValue;
        }

        public override void WriteToIni(INIParser iniparse)
        {
            iniparse.WriteValue(categoryKey, saveKey, MoonscraperEngine.EnumX<EnumType>.ToInt(value));
        }

        public override void ReadFromIni(INIParser iniparse)
        {
            value = MoonscraperEngine.EnumX<EnumType>.FromInt(iniparse.ReadValue(categoryKey, saveKey, MoonscraperEngine.EnumX<EnumType>.ToInt(defaultValue)));
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    // Non-saved settings
    public bool keysModeEnabled = false;
    public bool metronomeActive = false;
    public bool clapEnabled = false;
    public float gameSpeed = 1;

    // Custom accessors around private save settings for finer control
    public DrumModeOptions drumsModeOptions
    {
        get
        {
            return (DrumModeOptions)_drumsModeOptions.value;
        }
        set
        {
            _drumsModeOptions.value = (int)value;
            if (!Enum.IsDefined(typeof(DrumModeOptions), drumsModeOptions))
            {
                _drumsModeOptions.value = _drumsModeOptions.defaultValue;
            }
        }
    }

    public float sfxVolume
    {
        get { return _sfxVolume; }
        set
        {
            if (value < 0)
                _sfxVolume.value = 0;
            else if (value > 1)
                _sfxVolume.value = 1;
            else
                _sfxVolume.value = value;
        }
    }

    // These fields are automatically saved via reflection
    public BoolSaveSetting 
        slowdownPitchCorrectionEnabled
        , extendedSustainsEnabled
        , sustainGapEnabled
        , sustainGapIsTimeBased
        , resetAfterPlay
        , resetAfterGameplay
        , autoValidateSongOnSave
        , automaticallyCheckForUpdates
    ;

    public IntSaveSetting
        audioCalibrationMS
        , clapCalibrationMS
        , customBgSwapTime
        , targetFramerate
        , drumsLaneCount
        , sustainGapTimeMs
        , newSongResolution
    ;

    IntSaveSetting 
        _drumsModeOptions
    ;

    public FloatSaveSetting 
        hyperspeed
        , highwayLength
        , gameplayStartDelayTime
        , vol_master
        , vol_song
        , vol_guitar
        , vol_bass
        , vol_rhythm
        , vol_keys
        , vol_drums
        , vol_drums2
        , vol_drums3
        , vol_drums4
        , vol_vocals
        , vol_crowd
        , audio_pan
    ;

    FloatSaveSetting _sfxVolume;

    public EnumSaveSetting<ClapToggle> clapProperties;
    public EnumSaveSetting<NotePlacementMode> notePlacementMode;
    public EnumSaveSetting<SongValidate.ValidationOptions> songValidatorModes;

    public Step snappingStep = new Step(16);
    public int step { get { return snappingStep.value; } set { snappingStep.value = value; } }
    public Step sustainGapStep;
    public int sustainGap { get { return sustainGapStep.value; } set { sustainGapStep.value = value; } }

    public LyricEditorSettings lyricEditorSettings = new LyricEditorSettings();

    public MSChartEditorInput.MSChartEditorActionContainer controls = new MSChartEditorInput.MSChartEditorActionContainer();

    public bool GetBoolSetting(string identifier)
    {
        switch (identifier)
        {
            case ("keysModeEnabled"): return keysModeEnabled;
            case ("metronomeActive"): return metronomeActive;
            case ("extendedSustainsEnabled"): return extendedSustainsEnabled;
            case ("sustainGapEnabled"): return sustainGapEnabled;
            case ("resetAfterPlay"): return resetAfterPlay;
            case ("resetAfterGameplay"): return resetAfterGameplay;
            default: return false;
        }
    }

    public GameSettings()
    {
        slowdownPitchCorrectionEnabled = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Slowdown Pitch Correction Enabled", false);
        extendedSustainsEnabled = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Extended sustains", false);
        sustainGapEnabled = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Sustain Gap", false);
        sustainGapIsTimeBased = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Sustain Gap Is Time Based", false);
        resetAfterPlay = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Reset After Play", false);
        resetAfterGameplay = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Reset After Gameplay", false);
        autoValidateSongOnSave = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Auto Validate Song On Save", true);
        automaticallyCheckForUpdates = new BoolSaveSetting(SECTION_NAME_SETTINGS, "Auto Check For Updates", true);

        audioCalibrationMS = new IntSaveSetting(SECTION_NAME_SETTINGS, "Audio calibration", 0);
        clapCalibrationMS = new IntSaveSetting(SECTION_NAME_SETTINGS, "Clap calibration", 0);
        customBgSwapTime = new IntSaveSetting(SECTION_NAME_SETTINGS, "Custom Background Swap Time", 30);
        targetFramerate = new IntSaveSetting(SECTION_NAME_SETTINGS, "Framerate", 120);
        drumsLaneCount = new IntSaveSetting(SECTION_NAME_SETTINGS, "Drums Lane Count", 5);
        sustainGapTimeMs = new IntSaveSetting(SECTION_NAME_SETTINGS, "Sustain Gap Time", 0);
        newSongResolution = new IntSaveSetting(SECTION_NAME_SONG, "New Song Resolution", (int)MoonscraperChartEditor.Song.SongConfig.STANDARD_BEAT_RESOLUTION);
        _drumsModeOptions = new IntSaveSetting(SECTION_NAME_SETTINGS, "Drums Mode", (int)DrumModeOptions.Standard);

        hyperspeed = new FloatSaveSetting(SECTION_NAME_SETTINGS, "Hyperspeed", 5.0f);
        highwayLength = new FloatSaveSetting(SECTION_NAME_SETTINGS, "Highway Length", 0);
        gameplayStartDelayTime = new FloatSaveSetting(SECTION_NAME_SETTINGS, "Gameplay Start Delay", 3.0f);
        vol_master = new FloatSaveSetting(SECTION_NAME_AUDIO, "Master", 0.5f);
        vol_song = new FloatSaveSetting(SECTION_NAME_AUDIO, "Music Stream", 1.0f);
        vol_guitar = new FloatSaveSetting(SECTION_NAME_AUDIO, "Guitar Stream", 1.0f);
        vol_bass = new FloatSaveSetting(SECTION_NAME_AUDIO, "Bass Stream", 1.0f);
        vol_rhythm = new FloatSaveSetting(SECTION_NAME_AUDIO, "Rhythm Stream", 1.0f);
        vol_keys = new FloatSaveSetting(SECTION_NAME_AUDIO, "Keys Stream", 1.0f);
        vol_drums = new FloatSaveSetting(SECTION_NAME_AUDIO, "Drum Stream", 1.0f);
        vol_drums2 = new FloatSaveSetting(SECTION_NAME_AUDIO, "Drum 2 Stream", 1.0f);
        vol_drums3 = new FloatSaveSetting(SECTION_NAME_AUDIO, "Drum 3 Stream", 1.0f);
        vol_drums4 = new FloatSaveSetting(SECTION_NAME_AUDIO, "Drum 4 Stream", 1.0f);
        vol_vocals = new FloatSaveSetting(SECTION_NAME_AUDIO, "Vocals Stream", 1.0f);
        vol_crowd = new FloatSaveSetting(SECTION_NAME_AUDIO, "Crowd Stream", 1.0f);
        audio_pan = new FloatSaveSetting(SECTION_NAME_AUDIO, "Audio Pan", 0.0f);
        _sfxVolume = new FloatSaveSetting(SECTION_NAME_AUDIO, "SFX", 1.0f);

        clapProperties = new EnumSaveSetting<ClapToggle>(SECTION_NAME_SETTINGS, "Clap", ClapToggle.ALL_NOTES | ClapToggle.STRUM | ClapToggle.HOPO | ClapToggle.TAP);
        notePlacementMode = new EnumSaveSetting<NotePlacementMode>(SECTION_NAME_SETTINGS, "Note Placement Mode", NotePlacementMode.Default);
        songValidatorModes = new EnumSaveSetting<SongValidate.ValidationOptions>(SECTION_NAME_SETTINGS, "Song Validator Modes", ~SongValidate.ValidationOptions.None);
    }

    public void Load(string configFilepath, string controllerBindingsFilepath)
    {
        INIParser iniparse = new INIParser();

        try
        {
            Debug.Log("Loading game settings");
         
            iniparse.Open(configFilepath);

            int versionNumber = iniparse.ReadValue(SECTION_NAME_METADATA, "Version Number", VersionNumber);

            // Check for valid fps values
            clapEnabled = false;
            sustainGapStep = new Step((int)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Sustain Gap Step", (int)16));

            // Graphics Settings
            QualitySettings.antiAliasing = iniparse.ReadValue(SECTION_NAME_GRAPHICS, "AntiAliasingLevel", QualitySettings.antiAliasing);

            lyricEditorSettings.stepSnappingEnabled = iniparse.ReadValue(SECTION_NAME_LYRICEDITOR, "Step Snapping", true);
            lyricEditorSettings.phaseEndThreashold = (float)iniparse.ReadValue(SECTION_NAME_LYRICEDITOR, "Phase End Threashold", 0f);

            // Read save settings
            foreach (var prop in GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Type type = GetUnderlyingType(prop);

                if (type.IsSubclassOf(typeof(SaveSetting)))
                {
                    SaveSetting setting = (SaveSetting)GetValue(prop, this);
                    setting.ReadFromIni(iniparse);
                    SetValue(prop, this, setting);
                }
            }

            if (versionNumber < 1)
            {
                // Need to fix old config values
                if ((int)clapProperties.value > (((int)ClapToggle.SECTION << 1) - 1))
                {
                    clapProperties.value = clapProperties.defaultValue;
                }
            }

            gameplayStartDelayTime.value = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
            gameplayStartDelayTime.value = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f); // Check that the gameplay start delay time is a multiple of 0.5 and is
        }
        catch (Exception e)
        {
            Debug.LogError("Error encountered when trying to load game settings. " + e.Message);
        }
        finally
        {
            iniparse.Close();
        }

        try
        {
            // Populate with all default controls first
            controls.LoadFromSaveData(InputManager.Instance.defaultControls);

            if (File.Exists(controllerBindingsFilepath))
            {
                // Override with custom controls if they exist.
                Debug.Log("Loading input settings");
                string controlsJson = File.ReadAllText(controllerBindingsFilepath);
                controls.LoadFromSaveData(JsonUtility.FromJson<MSChartEditorInput.MSChartEditorActionContainer>(controlsJson));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to load saved controls. " + e.Message);
        }
    }

    public void Save(string configFilepath, string controllerBindingsFilepath)
    {
        INIParser iniparse = new INIParser();

        try
        {
            Debug.Log("Saving game settings");

            iniparse.Open(configFilepath);
            iniparse.WriteValue(SECTION_NAME_METADATA, "Version Number", VersionNumber);

            iniparse.WriteValue(SECTION_NAME_SETTINGS, "Sustain Gap Step", sustainGap);

            // Graphics Settings
            iniparse.WriteValue(SECTION_NAME_GRAPHICS, "AntiAliasingLevel", QualitySettings.antiAliasing);

            // Lyric Editor Settings
            iniparse.WriteValue(SECTION_NAME_LYRICEDITOR, "Step Snapping", lyricEditorSettings.stepSnappingEnabled);
            iniparse.WriteValue(SECTION_NAME_LYRICEDITOR, "Phase End Threashold", lyricEditorSettings.phaseEndThreashold);

            // Write save settings
            foreach (var prop in GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Type type = GetUnderlyingType(prop);

                if (type.IsSubclassOf(typeof(SaveSetting)))
                {
                    SaveSetting setting = (SaveSetting)GetValue(prop, this);
                    setting.WriteToIni(iniparse);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error encountered when trying to save game settings. " + e.Message);
        }
        finally
        {
            iniparse.Close();
        }

        controls.UpdateSaveData();
        var controlsJson = JsonUtility.ToJson(controls, true);
        try
        {
            Debug.Log("Saving input settings");

            // Save to file
            File.WriteAllText(controllerBindingsFilepath, controlsJson, System.Text.Encoding.UTF8);
        }
        catch (System.Exception e)
        {
            Logger.LogException(e, "Error encountered while saving control bindings. " + e.Message);
        }
    }

    static object GetValue(MemberInfo mi, object targetObject)
    {
        switch (mi.MemberType)
        {
            case MemberTypes.Field:
                try
                {
                    return (mi as FieldInfo).GetValue(targetObject);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Could not get field {0} on object of type {1}. Error {2}", mi.Name, targetObject.GetType(), e.Message));
                }
                break;

            case MemberTypes.Property:
                try
                {
                    return (mi as PropertyInfo).GetValue(targetObject);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Could not get property {0} on object of type {1}. Error {2}", mi.Name, targetObject.GetType(), e.Message));
                }

                break;
            default:
                Debug.LogError(string.Format("Could not get property {0} on object of type {1}. MemberInfo must be a subtype of FieldInfo or PropertyInfo.", mi.Name, targetObject.GetType()));
                break;
        }

        return null;
    }

    static void SetValue(MemberInfo mi, object targetObject, object value)
    {
        switch (mi.MemberType)
        {
            case MemberTypes.Field:
                try
                {
                    (mi as FieldInfo).SetValue(targetObject, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Could not set field {0} on object of type {1}. Error {2}", mi.Name, targetObject.GetType(), e.Message));
                }
                break;

            case MemberTypes.Property:
                try
                {
                    (mi as PropertyInfo).SetValue(targetObject, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Could not set property {0} on object of type {1}. Error {2}", mi.Name, targetObject.GetType(), e.Message));
                }

                break;
            default:
                Debug.LogError(string.Format("Could not set property {0} on object of type {1}. MemberInfo must be a subtype of FieldInfo or PropertyInfo.", mi.Name, targetObject.GetType()));
                break;
        }
    }

    public static Type GetUnderlyingType(MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Event:
                return ((EventInfo)member).EventHandlerType;
            case MemberTypes.Field:
                return ((FieldInfo)member).FieldType;
            case MemberTypes.Method:
                return ((MethodInfo)member).ReturnType;
            case MemberTypes.Property:
                return ((PropertyInfo)member).PropertyType;
            default:
                return typeof(object);
        }
    }
}

