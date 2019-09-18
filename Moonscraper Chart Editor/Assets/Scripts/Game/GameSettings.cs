using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MSE.Input;

public static class GameSettings
{
    const string SECTION_NAME_SETTINGS = "Settings";
    const string SECTION_NAME_AUDIO = "Audio Volume";

    [System.Flags]
    public enum ClapToggle
    {
        NONE = 0,
        
        STRUM           = 1 << 0,
        HOPO            = 1 << 1,
        TAP             = 1 << 2,

        STARPOWER       = 1 << 3,
        CHARTEVENT      = 1 << 4,

        BPM             = 1 << 5,
        TS              = 1 << 6,
        EVENT           = 1 << 7,
        SECTION         = 1 << 8,
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

    public static bool clapEnabled = false;
    public static ClapToggle clapProperties = ClapToggle.NONE;
    public static NotePlacementMode notePlacementMode = NotePlacementMode.LeftyFlip;

    public static ShortcutInput.ShortcutActionContainer controls = new ShortcutInput.ShortcutActionContainer();

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

    static GameSettings()
    {
        LoadDefaultControls(controls);
    }

    public static void Load(string configFilepath, string controllerBindingsFilepath)
    {
        INIParser iniparse = new INIParser();

        try
        {
            Debug.Log("Loading game settings");

            int c_defaultClapVal = (int)(ClapToggle.STRUM | ClapToggle.HOPO | ClapToggle.TAP);
         
            iniparse.Open(configFilepath);

            // Check for valid fps values
            targetFramerate = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Framerate", 120);
            hyperspeed = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Hyperspeed", 5.0f);
            highwayLength = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Highway Length", 0);
            audioCalibrationMS = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Audio calibration", 0);
            clapCalibrationMS = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Clap calibration", 0);
            clapProperties = (ClapToggle)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Clap", c_defaultClapVal);
            extendedSustainsEnabled = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Extended sustains", false);
            clapEnabled = false;
            sustainGapEnabled = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Sustain Gap", false);
            sustainGapStep = new Step((int)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Sustain Gap Step", (int)16));
            notePlacementMode = (NotePlacementMode)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Note Placement Mode", (int)NotePlacementMode.Default);
            gameplayStartDelayTime = (float)iniparse.ReadValue(SECTION_NAME_SETTINGS, "Gameplay Start Delay", 3.0f);
            resetAfterPlay = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Reset After Play", false);
            resetAfterGameplay = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Reset After Gameplay", false);
            customBgSwapTime = iniparse.ReadValue(SECTION_NAME_SETTINGS, "Custom Background Swap Time", 30);
            gameplayStartDelayTime = Mathf.Clamp(gameplayStartDelayTime, 0, 3.0f);
            gameplayStartDelayTime = (float)(System.Math.Round(gameplayStartDelayTime * 2.0f, System.MidpointRounding.AwayFromZero) / 2.0f); // Check that the gameplay start delay time is a multiple of 0.5 and is

            // Audio levels
            vol_master = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Master", 0.5f);
            vol_song = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Music Stream", 1.0f);
            vol_guitar = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Guitar Stream", 1.0f);
            vol_bass = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Bass Stream", 1.0f);
            vol_rhythm = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Rhythm Stream", 1.0f);
            vol_drum = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Drum Stream", 1.0f);
            audio_pan = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "Audio Pan", 0.0f);
            sfxVolume = (float)iniparse.ReadValue(SECTION_NAME_AUDIO, "SFX", 1.0f);

            // Need to fix old config values
            if ((int)clapProperties > (((int)ClapToggle.SECTION << 1) - 1))
            {
                clapProperties = (ClapToggle)c_defaultClapVal;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error encountered when trying to load game settings. " + e.Message);
        }
        finally
        {
            iniparse.Close();
        }

        try
        {
            if (File.Exists(controllerBindingsFilepath))
            {
                Debug.Log("Loading input settings");
                string controlsJson = File.ReadAllText(controllerBindingsFilepath);
                controls.LoadFromSaveData(JsonUtility.FromJson<ShortcutInput.ShortcutActionContainer>(controlsJson));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unable to load saved controls. " + e.Message);
        }
    }

    public static void Save(string configFilepath, string controllerBindingsFilepath)
    {
        INIParser iniparse = new INIParser();

        try
        {
            Debug.Log("Saving game settings");

            iniparse.Open(configFilepath);

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
        }
        catch (System.Exception e)
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

    public static void LoadDefaultControls(ShortcutInput.ShortcutActionContainer inputList)
    {
        SetDefaultKeysControls(inputList);
    }

    public static void SetDefaultKeysControls(ShortcutInput.ShortcutActionContainer inputList)
    {
        {
            inputList.GetActionConfig(Shortcut.AddSongObject).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha1 };
            inputList.GetActionConfig(Shortcut.BpmIncrease).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Equals, };
            inputList.GetActionConfig(Shortcut.BpmDecrease).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Minus, };
            inputList.GetActionConfig(Shortcut.Delete).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Delete };
            inputList.GetActionConfig(Shortcut.PlayPause).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Space };
            inputList.GetActionConfig(Shortcut.MoveStepPositive).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.UpArrow };
            inputList.GetActionConfig(Shortcut.MoveStepNegative).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.DownArrow };
            inputList.GetActionConfig(Shortcut.MoveMeasurePositive).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.PageUp };
            inputList.GetActionConfig(Shortcut.MoveMeasureNegative).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.PageDown };
            inputList.GetActionConfig(Shortcut.NoteSetNatural).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.X };
            inputList.GetActionConfig(Shortcut.NoteSetStrum).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.S };
            inputList.GetActionConfig(Shortcut.NoteSetHopo).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.H };
            inputList.GetActionConfig(Shortcut.NoteSetTap).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.T };
            inputList.GetActionConfig(Shortcut.StepIncrease).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.W };
            inputList.GetActionConfig(Shortcut.StepIncrease).inputMaps.kbMaps[1] = new KeyboardMap() { KeyCode.RightArrow };
            inputList.GetActionConfig(Shortcut.StepDecrease).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Q };
            inputList.GetActionConfig(Shortcut.StepDecrease).inputMaps.kbMaps[1] = new KeyboardMap() { KeyCode.LeftArrow };
            inputList.GetActionConfig(Shortcut.ToggleBpmAnchor).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.A };
            inputList.GetActionConfig(Shortcut.ToggleClap).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.N };
            inputList.GetActionConfig(Shortcut.ToggleExtendedSustains).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.E };
            inputList.GetActionConfig(Shortcut.ToggleMetronome).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.M };
            inputList.GetActionConfig(Shortcut.ToggleMouseMode).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.BackQuote };
            inputList.GetActionConfig(Shortcut.ToggleNoteForced).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.F };
            inputList.GetActionConfig(Shortcut.ToggleNoteTap).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.T };
            inputList.GetActionConfig(Shortcut.ToggleViewMode).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.G };
            inputList.GetActionConfig(Shortcut.ToolNoteBurst).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.B };
            inputList.GetActionConfig(Shortcut.ToolNoteHold).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.H };
            inputList.GetActionConfig(Shortcut.ToolSelectCursor).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.J };
            inputList.GetActionConfig(Shortcut.ToolSelectEraser).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.K };
            inputList.GetActionConfig(Shortcut.ToolSelectNote).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Y };
            inputList.GetActionConfig(Shortcut.ToolSelectStarpower).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.U };
            inputList.GetActionConfig(Shortcut.ToolSelectBpm).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.I };
            inputList.GetActionConfig(Shortcut.ToolSelectTimeSignature).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.O };
            inputList.GetActionConfig(Shortcut.ToolSelectSection).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.P };
            inputList.GetActionConfig(Shortcut.ToolSelectEvent).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.L };

            inputList.GetActionConfig(Shortcut.ToolNoteLane1).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha1 };
            inputList.GetActionConfig(Shortcut.ToolNoteLane2).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha2 };
            inputList.GetActionConfig(Shortcut.ToolNoteLane3).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha3 };
            inputList.GetActionConfig(Shortcut.ToolNoteLane4).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha4 };
            inputList.GetActionConfig(Shortcut.ToolNoteLane5).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha5 };
            inputList.GetActionConfig(Shortcut.ToolNoteLane6).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha6 };
            inputList.GetActionConfig(Shortcut.ToolNoteLaneOpen).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Alpha0 };

            inputList.GetActionConfig(Shortcut.CloseMenu).inputMaps.kbMaps[0] = new KeyboardMap() { KeyCode.Escape };
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Ctrl;
            inputList.GetActionConfig(Shortcut.ClipboardCopy).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.C };
            inputList.GetActionConfig(Shortcut.ClipboardCut).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.X };
            inputList.GetActionConfig(Shortcut.ClipboardPaste).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.V };
            inputList.GetActionConfig(Shortcut.FileLoad).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.O };
            inputList.GetActionConfig(Shortcut.FileNew).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.N };
            inputList.GetActionConfig(Shortcut.FileSave).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.S };
            inputList.GetActionConfig(Shortcut.ActionHistoryRedo).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.Y };
            inputList.GetActionConfig(Shortcut.ActionHistoryUndo).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.Z };
            inputList.GetActionConfig(Shortcut.SelectAll).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.A };
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Shift;

            inputList.GetActionConfig(Shortcut.ChordSelect).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { };
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Ctrl | KeyboardDevice.ModifierKeys.Shift;

            inputList.GetActionConfig(Shortcut.FileSaveAs).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.S };
            inputList.GetActionConfig(Shortcut.ActionHistoryRedo).inputMaps.kbMaps[1] = new KeyboardMap(modiInput) { KeyCode.Z };
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Alt;

            inputList.GetActionConfig(Shortcut.SectionJumpPositive).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.UpArrow };
            inputList.GetActionConfig(Shortcut.SectionJumpNegative).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.DownArrow };
            inputList.GetActionConfig(Shortcut.SelectAllSection).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.A };
            inputList.GetActionConfig(Shortcut.SectionJumpMouseScroll).inputMaps.kbMaps[0] = new KeyboardMap(modiInput) { };
        }
    }
}

