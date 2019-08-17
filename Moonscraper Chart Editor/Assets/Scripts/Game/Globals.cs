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
            return ChartEditor.Instance.currentChart.gameMode == Chart.GameMode.Drums;
        }
    }
    public static bool ghLiveMode
    {
        get
        {
            return ChartEditor.Instance.currentChart.gameMode == Chart.GameMode.GHLGuitar;
        }
    }

    public static ViewMode viewMode { get; set; }

    ChartEditor editor;
    Services _services;
    public Services services { get { return _services; } }
    Resolution largestRes;

    void Awake()
    {
        Application.runInBackground = true;

        largestRes = Screen.resolutions[0];
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width > largestRes.width)
                largestRes = res;
        }
        autosaveLocation = Application.persistentDataPath + "/autosave.chart";

        viewMode = ViewMode.Chart;
        editor = ChartEditor.Instance;
        _services = GetComponent<Services>();

#if !UNITY_EDITOR
        workingDirectory = DirectoryHelper.GetMainDirectory();
#else
        workingDirectory = Application.dataPath;
#endif
        // Bass init
        AudioManager.Init();

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
            string autosaveText = "An autosave was detected indicating that the program did not correctly shut down during the last session. \nWould you like to reload the autosave?";
            string autosaveCaption = "Warning";

            NativeMessageBox.Result result = NativeMessageBox.Show(autosaveText, autosaveCaption, NativeMessageBox.Type.YesNo);

            if (result == NativeMessageBox.Result.Yes)
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
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && Services.IsTyping && editor.currentState == ChartEditor.State.Editor)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public static bool modifierInputActive { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand); } }
    public static bool secondaryInputActive { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }

    void Shortcuts()
    {
        if (ShortcutInput.GetInputDown(Shortcut.PlayPause))
        {
            if (editor.currentState == ChartEditor.State.Editor && services.CanPlay())
                editor.Play();
            else if (editor.currentState == ChartEditor.State.Playing)
                editor.Stop();
        }

        else if (ShortcutInput.GetInputDown(Shortcut.StepIncrease))
            GameSettings.snappingStep.Increment();

        else if (ShortcutInput.GetInputDown(Shortcut.StepDecrease))
            GameSettings.snappingStep.Decrement();

        else if (ShortcutInput.GetInputDown(Shortcut.Delete) && editor.selectedObjectsManager.currentSelectedObjects.Count > 0)
            editor.Delete();

        else if (ShortcutInput.GetInputDown(Shortcut.ToggleMetronome))
        {
            services.ToggleMetronome();
            services.notificationBar.PushNotification("METRONOME TOGGLED " + Services.BoolToStrOnOff(GameSettings.metronomeActive), 2, true);
        }

        if (!modifierInputActive)
        {
            if (editor.inputManager.mainGamepad.GetButtonPressed(GamepadInput.Button.Start))
            {
                if (editor.currentState != ChartEditor.State.Playing)
                    editor.StartGameplay();
                else
                    editor.Stop();
            }
        }

        if (ShortcutInput.GetInputDown(Shortcut.FileSave))
            editor._Save();

        else if (ShortcutInput.GetInputDown(Shortcut.FileSaveAs))
            editor.SaveAs();

        else if (ShortcutInput.GetInputDown(Shortcut.FileLoad))
            editor.Load();

        else if (ShortcutInput.GetInputDown(Shortcut.FileNew))
            editor.New();

        else if (ShortcutInput.GetInputDown(Shortcut.SelectAll))
        {
            services.toolpanelController.SetCursor();
            editor.selectedObjectsManager.SelectAllInView(viewMode);
        }
        else if (ShortcutInput.GetInputDown(Shortcut.SelectAllSection))
        {
            services.toolpanelController.SetCursor();
            editor.selectedObjectsManager.HighlightCurrentSection(viewMode);
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            bool success = false;

            if (ShortcutInput.GetInputDown(Shortcut.ActionHistoryUndo))
            {
                if (!editor.commandStack.isAtStart && editor.services.CanUndo())
                {
                    editor.UndoWrapper();
                    success = true;
                }
            }
            else if (ShortcutInput.GetInputDown(Shortcut.ActionHistoryRedo))
            {
                if (!editor.commandStack.isAtEnd && editor.services.CanRedo())
                {
                    editor.RedoWrapper();
                    success = true;
                }
            }

            if (success)
            {
                EventSystem.current.SetSelectedGameObject(null);
                groupSelect.reset();
                TimelineHandler.Repaint();
            }
        }

        if (editor.selectedObjectsManager.currentSelectedObjects.Count > 0)
        {
            if (ShortcutInput.GetInputDown(Shortcut.ClipboardCut))
                editor.Cut();
            else if (ShortcutInput.GetInputDown(Shortcut.ClipboardCopy))
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

    public enum ViewMode
    {
        Chart, Song
    }
}
