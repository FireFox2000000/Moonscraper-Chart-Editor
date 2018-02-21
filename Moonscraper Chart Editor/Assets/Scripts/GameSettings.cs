using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    const string SECTION_NAME_SETTINGS = "Settings";
    const string SECTION_NAME_AUDIO = "Audio Volume";

    [System.Flags]
    public enum ClapToggle
    {
        NONE = 0, ALL = ~0, STRUM = 1, HOPO = 2, TAP = 4
    }

    public enum NotePlacementMode
    {
        Default, LeftyFlip
    }

    public static bool keysModeEnabled = false;
    public static bool metronomeActive = false;
    public static bool extendedSustainsEnabled = false;
    public static bool sustainGapEnabled { get; set; }
    public static bool resetAfterPlay = false;
    public static bool resetAfterGameplay = false;
    public static bool bot = true;

    public static int audioCalibrationMS = 200;                     // Increase to start the audio sooner
    public static int clapCalibrationMS = 200;
    public static int customBgSwapTime;
    public static int targetFramerate = -1;

    public static float hyperspeed = 5.0f;
    public static float highwayLength = 0;
    static float _sfxVolume = 1;
    public static float gameSpeed = 1;
    public static float gameplayStartDelayTime = 3.0f;
    public static float sfxVolume
    {
        get { return _sfxVolume; }
        set
        {
            if (value < 0)
                _sfxVolume = 0;
            else if (value > 1)
                _sfxVolume = 1;
            else
                _sfxVolume = value;
        }
    }
    public static float vol_master, vol_song, vol_guitar, vol_bass, vol_rhythm, vol_drum, audio_pan;

    public static Step snappingStep = new Step(16);
    public static int step { get { return snappingStep.value; } set { snappingStep.value = value; } }
    public static Step sustainGapStep;
    public static int sustainGap { get { return sustainGapStep.value; } set { sustainGapStep.value = value; } }

    public static ClapToggle clapSetting = ClapToggle.NONE;
    public static ClapToggle clapProperties = ClapToggle.NONE;
    public static NotePlacementMode notePlacementMode = NotePlacementMode.LeftyFlip;

    public static bool GetBoolSetting(string identifier)
    {
        switch (identifier)
        {
            case ("keysModeEnabled"): return keysModeEnabled;
            case ("metronomeActive"): return metronomeActive;
            case ("extendedSustainsEnabled"): return extendedSustainsEnabled;
            case ("sustainGapEnabled"): return sustainGapEnabled;
            case ("resetAfterPlay"): return resetAfterPlay;
            case ("resetAfterGameplay"): return resetAfterGameplay;
            case ("bot"): return bot;
            default: return false;
        }
    }

    public static void Load(string filepath)
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(filepath);

        // Check for valid fps values
        targetFramerate                 = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Framerate", 120);
        hyperspeed                      = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Hyperspeed", 5.0f);
        highwayLength                   = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Highway Length", 0);
        audioCalibrationMS              = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Audio calibration", 0);
        clapCalibrationMS               = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Clap calibration", 0);
        clapProperties                  = (ClapToggle)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Clap", (int)ClapToggle.ALL);
        extendedSustainsEnabled         = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Extended sustains", false);
        clapSetting                     = ClapToggle.NONE;
        sustainGapEnabled               = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Sustain Gap", false);
        sustainGapStep                  = new Step((int)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Sustain Gap Step", (int)16));
        notePlacementMode               = (NotePlacementMode)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Note Placement Mode", (int)NotePlacementMode.Default);
        gameplayStartDelayTime          = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Gameplay Start Delay", 3.0f);
        resetAfterPlay                  = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Reset After Play", false);
        resetAfterGameplay              = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Reset After Gameplay", false);
        customBgSwapTime                = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Custom Background Swap Time", 30); 
        gameplayStartDelayTime          = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
        gameplayStartDelayTime          = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f); // Check that the gameplay start delay time is a multiple of 0.5 and is

        // Audio levels
        vol_master                      = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Master", 0.5f);
        vol_song                        = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Music Stream", 1.0f);
        vol_guitar                      = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Guitar Stream", 1.0f);
        vol_bass                        = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Bass Stream", 1.0f);
        vol_rhythm                      = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Rhythm Stream", 1.0f);
        vol_drum                        = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Drum Stream", 1.0f);
        audio_pan                       = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Audio Pan", 0.0f);
        sfxVolume                       = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "SFX", 1.0f);

        iniparse.Close();
    }

    public static void Save(string filepath)
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(filepath);

        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Framerate", targetFramerate);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Hyperspeed", hyperspeed);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Highway Length", highwayLength);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Audio calibration", audioCalibrationMS);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Clap calibration", clapCalibrationMS);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Clap", (int)clapProperties);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Extended sustains", extendedSustainsEnabled);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Sustain Gap", sustainGapEnabled);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Sustain Gap Step", sustainGap);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Note Placement Mode", (int)notePlacementMode);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Gameplay Start Delay", gameplayStartDelayTime);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Reset After Play", resetAfterPlay);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Reset After Gameplay", resetAfterGameplay);
        iniparse.WriteValue(SECTION_NAME_SETTINGS, "Custom Background Swap Time", customBgSwapTime);

        // Audio levels
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Master", vol_master);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Music Stream", vol_song);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Guitar Stream", vol_guitar);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Bass Stream", vol_bass);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Rhythm Stream", vol_rhythm);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Drum Stream", vol_drum);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "Audio Pan", audio_pan);
        iniparse.WriteValue(SECTION_NAME_AUDIO, "SFX", sfxVolume);

        iniparse.Close();
    }
}

