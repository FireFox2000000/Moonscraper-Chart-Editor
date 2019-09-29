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

    public static int audioCalibrationMS = 0;                     // Increase to start the audio sooner
    public static int clapCalibrationMS = 0;
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

    const int c_defaultClapVal = (int)(ClapToggle.STRUM | ClapToggle.HOPO | ClapToggle.TAP);
    public static ClapToggle clapProperties = (ClapToggle)c_defaultClapVal;
    public static NotePlacementMode notePlacementMode = NotePlacementMode.Default;

    public static MSChartEditorInput.MSChartEditorActionContainer controls = new MSChartEditorInput.MSChartEditorActionContainer();

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
        LoadDefaultControls();
    }

    public static void Load(string configFilepath, string controllerBindingsFilepath)
    {
        INIParser iniparse = new INIParser();

        try
        {
            Debug.Log("Loading game settings");
         
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
                controls.LoadFromSaveData(JsonUtility.FromJson<MSChartEditorInput.MSChartEditorActionContainer>(controlsJson));
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

    static void LoadDefaultControls()
    {
        SetDefaultEditorControls(controls);
        SetDefaultGameplayControls(controls);
    }

    public static void SetDefaultEditorControls(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        SetDefaultKeysControls(inputList);
        SetDefaultEditorControlsPad(inputList);
    }

    public static void SetDefaultKeysControls(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        // Reset all maps to a blank state
        foreach (MSChartEditorInputActions sc in EnumX<MSChartEditorInputActions>.Values)
        {
            var config = inputList.GetActionConfig(sc);
            if (((1 << config.properties.category) & MSChartEditorInput.Category.kEditorCategoryMask) != 0)
                config.RemoveMapsForDevice(MSE.Input.DeviceType.Keyboard);
        }

        {
            inputList.GetActionConfig(MSChartEditorInputActions.AddSongObject).Add(new KeyboardMap() { KeyCode.Alpha1 });
            inputList.GetActionConfig(MSChartEditorInputActions.BpmIncrease).Add(new KeyboardMap() { KeyCode.Equals, });
            inputList.GetActionConfig(MSChartEditorInputActions.BpmDecrease).Add(new KeyboardMap() { KeyCode.Minus, });
            inputList.GetActionConfig(MSChartEditorInputActions.Delete).Add(new KeyboardMap() { KeyCode.Delete });
            inputList.GetActionConfig(MSChartEditorInputActions.PlayPause).Add(new KeyboardMap() { KeyCode.Space });
            inputList.GetActionConfig(MSChartEditorInputActions.MoveStepPositive).Add(new KeyboardMap() { KeyCode.UpArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.MoveStepNegative).Add(new KeyboardMap() { KeyCode.DownArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.MoveMeasurePositive).Add(new KeyboardMap() { KeyCode.PageUp });
            inputList.GetActionConfig(MSChartEditorInputActions.MoveMeasureNegative).Add(new KeyboardMap() { KeyCode.PageDown });
            inputList.GetActionConfig(MSChartEditorInputActions.NoteSetNatural).Add(new KeyboardMap() { KeyCode.X });
            inputList.GetActionConfig(MSChartEditorInputActions.NoteSetStrum).Add(new KeyboardMap() { KeyCode.S });
            inputList.GetActionConfig(MSChartEditorInputActions.NoteSetHopo).Add(new KeyboardMap() { KeyCode.H });
            inputList.GetActionConfig(MSChartEditorInputActions.NoteSetTap).Add(new KeyboardMap() { KeyCode.T });
            inputList.GetActionConfig(MSChartEditorInputActions.StepIncrease).Add(new KeyboardMap() { KeyCode.W });
            inputList.GetActionConfig(MSChartEditorInputActions.StepIncrease).Add(new KeyboardMap() { KeyCode.RightArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.StepDecrease).Add(new KeyboardMap() { KeyCode.Q });
            inputList.GetActionConfig(MSChartEditorInputActions.StepDecrease).Add(new KeyboardMap() { KeyCode.LeftArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleBpmAnchor).Add(new KeyboardMap() { KeyCode.A });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleClap).Add(new KeyboardMap() { KeyCode.N });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleExtendedSustains).Add(new KeyboardMap() { KeyCode.E });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleMetronome).Add(new KeyboardMap() { KeyCode.M });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleMouseMode).Add(new KeyboardMap() { KeyCode.BackQuote });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleNoteForced).Add(new KeyboardMap() { KeyCode.F });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleNoteTap).Add(new KeyboardMap() { KeyCode.T });
            inputList.GetActionConfig(MSChartEditorInputActions.ToggleViewMode).Add(new KeyboardMap() { KeyCode.G });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteBurst).Add(new KeyboardMap() { KeyCode.B });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteHold).Add(new KeyboardMap() { KeyCode.H });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectCursor).Add(new KeyboardMap() { KeyCode.J });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectEraser).Add(new KeyboardMap() { KeyCode.K });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectNote).Add(new KeyboardMap() { KeyCode.Y });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectStarpower).Add(new KeyboardMap() { KeyCode.U });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectBpm).Add(new KeyboardMap() { KeyCode.I });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectTimeSignature).Add(new KeyboardMap() { KeyCode.O });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectSection).Add(new KeyboardMap() { KeyCode.P });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolSelectEvent).Add(new KeyboardMap() { KeyCode.L });

            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane1).Add(new KeyboardMap() { KeyCode.Alpha1 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane2).Add(new KeyboardMap() { KeyCode.Alpha2 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane3).Add(new KeyboardMap() { KeyCode.Alpha3 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane4).Add(new KeyboardMap() { KeyCode.Alpha4 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane5).Add(new KeyboardMap() { KeyCode.Alpha5 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLane6).Add(new KeyboardMap() { KeyCode.Alpha6 });
            inputList.GetActionConfig(MSChartEditorInputActions.ToolNoteLaneOpen).Add(new KeyboardMap() { KeyCode.Alpha0 });

            inputList.GetActionConfig(MSChartEditorInputActions.CloseMenu).Add(new KeyboardMap() { KeyCode.Escape });
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Ctrl;
            inputList.GetActionConfig(MSChartEditorInputActions.ClipboardCopy).Add(new KeyboardMap(modiInput) { KeyCode.C });
            inputList.GetActionConfig(MSChartEditorInputActions.ClipboardCut).Add(new KeyboardMap(modiInput) { KeyCode.X });
            inputList.GetActionConfig(MSChartEditorInputActions.ClipboardPaste).Add(new KeyboardMap(modiInput) { KeyCode.V });
            inputList.GetActionConfig(MSChartEditorInputActions.FileLoad).Add(new KeyboardMap(modiInput) { KeyCode.O });
            inputList.GetActionConfig(MSChartEditorInputActions.FileNew).Add(new KeyboardMap(modiInput) { KeyCode.N });
            inputList.GetActionConfig(MSChartEditorInputActions.FileSave).Add(new KeyboardMap(modiInput) { KeyCode.S });
            inputList.GetActionConfig(MSChartEditorInputActions.ActionHistoryRedo).Add(new KeyboardMap(modiInput) { KeyCode.Y });
            inputList.GetActionConfig(MSChartEditorInputActions.ActionHistoryUndo).Add(new KeyboardMap(modiInput) { KeyCode.Z });
            inputList.GetActionConfig(MSChartEditorInputActions.SelectAll).Add(new KeyboardMap(modiInput) { KeyCode.A });
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Shift;

            inputList.GetActionConfig(MSChartEditorInputActions.ChordSelect).Add(new KeyboardMap(modiInput) { });
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Ctrl | KeyboardDevice.ModifierKeys.Shift;

            inputList.GetActionConfig(MSChartEditorInputActions.FileSaveAs).Add(new KeyboardMap(modiInput) { KeyCode.S });
            inputList.GetActionConfig(MSChartEditorInputActions.ActionHistoryRedo).Add(new KeyboardMap(modiInput) { KeyCode.Z });
        }

        {
            KeyboardDevice.ModifierKeys modiInput = KeyboardDevice.ModifierKeys.Alt;

            inputList.GetActionConfig(MSChartEditorInputActions.SectionJumpPositive).Add(new KeyboardMap(modiInput) { KeyCode.UpArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.SectionJumpNegative).Add(new KeyboardMap(modiInput) { KeyCode.DownArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.SelectAllSection).Add(new KeyboardMap(modiInput) { KeyCode.A });
            inputList.GetActionConfig(MSChartEditorInputActions.SectionJumpMouseScroll).Add(new KeyboardMap(modiInput) { });
        }
    }

    public static void SetDefaultEditorControlsPad(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        // Reset all maps to a blank state
        foreach (MSChartEditorInputActions sc in EnumX<MSChartEditorInputActions>.Values)
        {
            var config = inputList.GetActionConfig(sc);
            if (((1 << config.properties.category) & MSChartEditorInput.Category.kEditorCategoryMask) != 0)
            {
                config.RemoveMapsForDevice(MSE.Input.DeviceType.Gamepad);
                config.Add(new GamepadButtonMap());  // Add empty maps
            }
        }
    }

    public static void SetDefaultGameplayControls(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        SetDefaultGameplayControlsPad(inputList);
        SetDefaultGameplayControlsKeys(inputList);
    }

    public static void SetDefaultGameplayControlsPad(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        // Reset all maps to a blank state
        foreach (MSChartEditorInputActions sc in EnumX<MSChartEditorInputActions>.Values)
        {
            var config = inputList.GetActionConfig(sc);
            if (((1 << config.properties.category) & MSChartEditorInput.Category.kGameplayCategoryMask) != 0)
                config.RemoveMapsForDevice(MSE.Input.DeviceType.Gamepad);
        }

        inputList.GetActionConfig(MSChartEditorInputActions.GuitarStrumUp).Add(new GamepadButtonMap() { GamepadDevice.Button.DPadUp });
        inputList.GetActionConfig(MSChartEditorInputActions.GuitarStrumDown).Add(new GamepadButtonMap() { GamepadDevice.Button.DPadDown });

        inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretGreen).Add(new GamepadButtonMap() { GamepadDevice.Button.A });
        inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretRed).Add(new GamepadButtonMap() { GamepadDevice.Button.B });
        inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretYellow).Add(new GamepadButtonMap() { GamepadDevice.Button.Y });
        inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretBlue).Add(new GamepadButtonMap() { GamepadDevice.Button.X });
        inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretOrange).Add(new GamepadButtonMap() { GamepadDevice.Button.LB });

        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadRed).Add(new GamepadButtonMap() { GamepadDevice.Button.B });
        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadYellow).Add(new GamepadButtonMap() { GamepadDevice.Button.Y });
        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadBlue).Add(new GamepadButtonMap() { GamepadDevice.Button.X });
        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadOrange).Add(new GamepadButtonMap() { GamepadDevice.Button.RB });
        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadGreen).Add(new GamepadButtonMap() { GamepadDevice.Button.A });
        inputList.GetActionConfig(MSChartEditorInputActions.DrumPadKick).Add(new GamepadButtonMap() { GamepadDevice.Button.LB });
    }

    public static void SetDefaultGameplayControlsKeys(MSChartEditorInput.MSChartEditorActionContainer inputList)
    {
        foreach (MSChartEditorInputActions sc in EnumX<MSChartEditorInputActions>.Values)
        {
            var config = inputList.GetActionConfig(sc);
            if (((1 << config.properties.category) & MSChartEditorInput.Category.kGameplayCategoryMask) != 0)
                config.RemoveMapsForDevice(MSE.Input.DeviceType.Keyboard);
        }

        {
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarStrumUp).Add(new KeyboardMap() { KeyCode.UpArrow });
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarStrumDown).Add(new KeyboardMap() { KeyCode.DownArrow });

            inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretGreen).Add(new KeyboardMap() { KeyCode.Alpha1 });
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretRed).Add(new KeyboardMap() { KeyCode.Alpha2 });
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretYellow).Add(new KeyboardMap() { KeyCode.Alpha3 });
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretBlue).Add(new KeyboardMap() { KeyCode.Alpha4 });
            inputList.GetActionConfig(MSChartEditorInputActions.GuitarFretOrange).Add(new KeyboardMap() { KeyCode.Alpha5 });

            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadRed).Add(new KeyboardMap() { KeyCode.Alpha1 });
            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadYellow).Add(new KeyboardMap() { KeyCode.Alpha2 });
            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadBlue).Add(new KeyboardMap() { KeyCode.Alpha3 });
            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadOrange).Add(new KeyboardMap() { KeyCode.Alpha4 });
            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadGreen).Add(new KeyboardMap() { KeyCode.Alpha5 });
            inputList.GetActionConfig(MSChartEditorInputActions.DrumPadKick).Add(new KeyboardMap() { KeyCode.Alpha0 });
        }
    }
}

