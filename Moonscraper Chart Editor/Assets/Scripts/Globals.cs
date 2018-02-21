// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.EventSystems;

public class Globals : MonoBehaviour {
    public static readonly string LINE_ENDING = "\r\n";
    public const string TABSPACE = "  ";
    public static string autosaveLocation;
    static string workingDirectory = string.Empty;
    public static string realWorkingDirectory { get { return workingDirectory; } }

    public static readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };
    public static readonly string[] validTextureExtensions = { ".jpg", ".png" };
    public static string[] localEvents = { };
    public static string[] globalEvents = { };

    [Header("Misc.")]
    [SerializeField]
    GroupSelect groupSelect;
    [SerializeField]
    Text snapLockWarning;
    [SerializeField]
    GUIStyle hintMouseOverStyle;

    public static bool drumMode
    {
        get
        {
            return MenuBar.currentInstrument == Song.Instrument.Drums;
        }
    }
    public static bool ghLiveMode
    {
        get
        {
            return MenuBar.currentInstrument == Song.Instrument.GHLiveGuitar || MenuBar.currentInstrument == Song.Instrument.GHLiveBass;
        }
    }

    // Settings
    public static ApplicationMode applicationMode = ApplicationMode.Editor;
    public static ViewMode viewMode { get; set; }

    ChartEditor editor;
    Services _services;
    public Services services { get { return _services; } }
    Resolution largestRes;

    void Awake()
    {
        largestRes = Screen.resolutions[0];
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width > largestRes.width)
                largestRes = res;
        }
        autosaveLocation = Application.persistentDataPath + "/autosave.chart";

        viewMode = ViewMode.Chart;
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        _services = GetComponent<Services>();
#if !UNITY_EDITOR
        workingDirectory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
#else
        workingDirectory = Application.dataPath;
#endif

        LoadGameSettings();

        localEvents = LoadCommonEvents("local_events.txt");
        globalEvents = LoadCommonEvents("global_events.txt");

        InputField[] allInputFields = Resources.FindObjectsOfTypeAll<InputField>();
        foreach (InputField inputField in allInputFields)
            inputField.gameObject.AddComponent<InputFieldDoubleClick>();

        HintMouseOver.style = hintMouseOverStyle;
    }

    void Start()
    {
        StartCoroutine(AutosaveCheck());
    }

    void LoadGameSettings()
    {
        GameSettings.Load(workingDirectory + "\\config.ini");

        // Check for valid fps values
        int fps = GameSettings.targetFramerate;
        if (fps != 60 && fps != 120 && fps != 240)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = fps;

        AudioListener.volume = GameSettings.vol_master;
    }

    static string[] LoadCommonEvents(string filename)
    {
#if UNITY_EDITOR
        string filepath = workingDirectory + "/ExtraBuildFiles/" + filename;
#else
        string filepath = workingDirectory + "/" + filename;
#endif
        Debug.Log(Path.GetFullPath(filepath));
        if (File.Exists(filepath))
        {
            Debug.Log("Loading events from " + filepath);

            StreamReader ifs = null;
            try
            {
                ifs = File.OpenText(filepath);
                var events = new System.Collections.Generic.List<string>();

                while (true)
                {
                    string line = ifs.ReadLine();
                    if (line == null)
                        break;

                    line.Replace('"', '\0');

                    if (line != string.Empty)
                        events.Add(line);
                }

                Debug.Log(events.Count + " event strings loaded");
                return events.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error: unable to load events- " + e.Message);
            }

            if (ifs != null)
                ifs.Close();
        }
        else
        {
            Debug.Log("No events file found. Skipping loading of default events.");
        }

        return new string[0];
    }

    IEnumerator AutosaveCheck()
    {
        yield return null;

        if (System.IO.File.Exists(autosaveLocation))
        {
#if !UNITY_EDITOR
            System.Windows.Forms.DialogResult result;

            result = System.Windows.Forms.MessageBox.Show("An autosave was detected indicating that the program did not corretly shut down during the last session. \nWould you like to reload the autosave?", 
                "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                yield return StartCoroutine(editor._Load(autosaveLocation, false));
                ChartEditor.isDirty = true;
            }
#endif
        }
    }
    
    void Update()
    {
        // Disable controls while user is in an input field
        Shortcuts();

        snapLockWarning.gameObject.SetActive((GameSettings.keysModeEnabled && Toolpane.currentTool != Toolpane.Tools.Cursor && Toolpane.currentTool != Toolpane.Tools.Eraser));

        // IsTyping can still be active if this isn't manually detected
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && Services.IsTyping && applicationMode == ApplicationMode.Editor)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public static bool modifierInputActive { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand); } }
    public static bool secondaryInputActive { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }

    void Shortcuts()
    {
        if (ShortcutMap.GetInputDown(Shortcut.PlayPause))
        {
            if (applicationMode == ApplicationMode.Editor)
                editor.Play();
            else if (applicationMode == ApplicationMode.Playing)
                editor.Stop();
        }

        else if (ShortcutMap.GetInputDown(Shortcut.StepIncrease))
            GameSettings.snappingStep.Increment();

        else if (ShortcutMap.GetInputDown(Shortcut.StepDecrease))
            GameSettings.snappingStep.Decrement();

        else if (ShortcutMap.GetInputDown(Shortcut.Delete) && editor.currentSelectedObjects.Length > 0)
            editor.Delete();

        else if (ShortcutMap.GetInputDown(Shortcut.ToggleMetronome))
        {
            services.ToggleMetronome();
            services.notificationBar.PushNotification("METRONOME TOGGLED " + Services.BoolToStrOnOff(GameSettings.metronomeActive), 2, true);
        }

        if (!modifierInputActive)
        {
#if true
            if (GameplayManager.gamepad != null && GameplayManager.previousGamepad != null &&
                ((XInputDotNetPure.GamePadState)GameplayManager.gamepad).Buttons.Start == XInputDotNetPure.ButtonState.Pressed &&
                ((XInputDotNetPure.GamePadState)GameplayManager.previousGamepad).Buttons.Start == XInputDotNetPure.ButtonState.Released)
#else
            if (Input.GetButtonDown("Start Gameplay"))
#endif
            {
                if (applicationMode != ApplicationMode.Playing)
                    editor.StartGameplay();
                else
                    editor.Stop();
            }
        }

        if (ShortcutMap.GetInputDown(Shortcut.FileSave))
            editor._Save();

        else if (ShortcutMap.GetInputDown(Shortcut.FileSaveAs))
            editor.SaveAs();

        else if (ShortcutMap.GetInputDown(Shortcut.FileLoad))
            editor.Load();

        else if (ShortcutMap.GetInputDown(Shortcut.FileNew))
            editor.New();

        else if (ShortcutMap.GetInputDown(Shortcut.SelectAll))
        {
            editor.currentSelectedObject = null;

            if (viewMode == ViewMode.Chart)
            {
                editor.currentSelectedObjects = editor.currentChart.chartObjects;
            }
            else
            {
                editor.currentSelectedObjects = editor.currentSong.syncTrack;
                editor.AddToSelectedObjects(editor.currentSong.eventsAndSections);
            }
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            bool success = false;

            if (ShortcutMap.GetInputDown(Shortcut.Undo))
            {
                success = editor.actionHistory.Undo(editor);
            }
            else if (ShortcutMap.GetInputDown(Shortcut.Redo))
            {
                success = editor.actionHistory.Redo(editor);
            }

            if (success)
            {
                EventSystem.current.SetSelectedGameObject(null);
                groupSelect.reset();
                TimelineHandler.externalUpdate = true;
            }
        }

        if (editor.currentSelectedObjects.Length > 0)
        {
            if (ShortcutMap.GetInputDown(Shortcut.ClipboardCut))
                editor.Cut();
            else if (ShortcutMap.GetInputDown(Shortcut.ClipboardCopy))
                editor.Copy();
        }
    }

    public void Quit()
    {
        GameSettings.targetFramerate = Application.targetFrameRate;
        GameSettings.Save(workingDirectory + "\\config.ini");

        // Delete autosaved chart. If chart is not deleted then that means there may have been a problem like a crash and the autosave should be reloaded the next time the program is opened. 
        if (File.Exists(autosaveLocation))
            File.Delete(autosaveLocation);
    }

    public static void DeselectCurrentUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ClickButton(Button button)
    {
        button.onClick.Invoke();
    }

    public enum ApplicationMode
    {
        Editor, Playing, Menu, Loading
    }

    public enum ViewMode
    {
        Chart, Song
    }
}
