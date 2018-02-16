using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
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
    public static float vol_master, vol_song, vol_guitar, vol_rhythm, vol_drum, audio_pan;

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
        targetFramerate                 = iniparse.ReadValue("Settings", "Framerate", 120);
        hyperspeed                      = (float)iniparse.ReadValue("Settings", "Hyperspeed", 5.0f);
        highwayLength                   = (float)iniparse.ReadValue("Settings", "Highway Length", 0);
        audioCalibrationMS              = iniparse.ReadValue("Settings", "Audio calibration", 0);
        clapCalibrationMS               = iniparse.ReadValue("Settings", "Clap calibration", 0);
        clapProperties                  = (ClapToggle)iniparse.ReadValue("Settings", "Clap", (int)ClapToggle.ALL);
        extendedSustainsEnabled         = iniparse.ReadValue("Settings", "Extended sustains", false);
        clapSetting                     = ClapToggle.NONE;
        sustainGapEnabled               = iniparse.ReadValue("Settings", "Sustain Gap", false);
        sustainGapStep                  = new Step((int)iniparse.ReadValue("Settings", "Sustain Gap Step", (int)16));
        notePlacementMode               = (NotePlacementMode)iniparse.ReadValue("Settings", "Note Placement Mode", (int)NotePlacementMode.Default);
        gameplayStartDelayTime          = (float)iniparse.ReadValue("Settings", "Gameplay Start Delay", 3.0f);
        resetAfterPlay                  = iniparse.ReadValue("Settings", "Reset After Play", false);
        resetAfterGameplay              = iniparse.ReadValue("Settings", "Reset After Gameplay", false);
        customBgSwapTime                = iniparse.ReadValue("Settings", "Custom Background Swap Time", 30); 
        gameplayStartDelayTime          = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
        gameplayStartDelayTime          = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f); // Check that the gameplay start delay time is a multiple of 0.5 and is

        // Audio levels
        vol_master                      = (float)iniparse.ReadValue("Audio Volume", "Master", 0.5f);
        vol_song                        = (float)iniparse.ReadValue("Audio Volume", "Music Stream", 1.0f);
        vol_guitar                      = (float)iniparse.ReadValue("Audio Volume", "Guitar Stream", 1.0f);
        vol_rhythm                      = (float)iniparse.ReadValue("Audio Volume", "Rhythm Stream", 1.0f);
        vol_drum                        = (float)iniparse.ReadValue("Audio Volume", "Drum Stream", 1.0f);
        audio_pan                       = (float)iniparse.ReadValue("Audio Volume", "Audio Pan", 0.0f);
        sfxVolume                       = (float)iniparse.ReadValue("Audio Volume", "SFX", 1.0f);

        iniparse.Close();
    }

    public static void Save(string filepath)
    {
        INIParser iniparse = new INIParser();
        iniparse.Open(filepath);

        iniparse.WriteValue("Settings", "Framerate", targetFramerate);
        iniparse.WriteValue("Settings", "Hyperspeed", hyperspeed);
        iniparse.WriteValue("Settings", "Highway Length", highwayLength);
        iniparse.WriteValue("Settings", "Audio calibration", audioCalibrationMS);
        iniparse.WriteValue("Settings", "Clap calibration", clapCalibrationMS);
        iniparse.WriteValue("Settings", "Clap", (int)clapProperties);
        iniparse.WriteValue("Settings", "Extended sustains", extendedSustainsEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap", sustainGapEnabled);
        iniparse.WriteValue("Settings", "Sustain Gap Step", sustainGap);
        iniparse.WriteValue("Settings", "Note Placement Mode", (int)notePlacementMode);
        iniparse.WriteValue("Settings", "Gameplay Start Delay", gameplayStartDelayTime);
        iniparse.WriteValue("Settings", "Reset After Play", resetAfterPlay);
        iniparse.WriteValue("Settings", "Reset After Gameplay", resetAfterGameplay);
        iniparse.WriteValue("Settings", "Custom Background Swap Time", customBgSwapTime);

        // Audio levels
        iniparse.WriteValue("Audio Volume", "Master", vol_master);
        iniparse.WriteValue("Audio Volume", "Music Stream", vol_song);
        iniparse.WriteValue("Audio Volume", "Guitar Stream", vol_guitar);
        iniparse.WriteValue("Audio Volume", "Rhythm Stream", vol_rhythm);
        iniparse.WriteValue("Audio Volume", "Drum Stream", vol_drum);
        iniparse.WriteValue("Audio Volume", "Audio Pan", audio_pan);
        iniparse.WriteValue("Audio Volume", "SFX", sfxVolume);

        iniparse.Close();
    }
}

