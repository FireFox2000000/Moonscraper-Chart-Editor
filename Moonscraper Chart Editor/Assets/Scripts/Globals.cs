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

    // Settings
    static ApplicationMode _applicationMode = ApplicationMode.Editor;
    public static ApplicationMode applicationMode
    {
        get
        {
            return _applicationMode;
        }
        set
        {
            _applicationMode = value;
            EventsManager.FireApplicationModeChangedEvent();
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
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        _services = GetComponent<Services>();

#if !UNITY_EDITOR
        workingDirectory = DirectoryHelper.GetMainDirectory();
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
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && Services.IsTyping && applicationMode == ApplicationMode.Editor)
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
            if (applicationMode == ApplicationMode.Editor)
                editor.Play();
            else if (applicationMode == ApplicationMode.Playing)
                editor.Stop();
        }

        else if (ShortcutInput.GetInputDown(Shortcut.StepIncrease))
            GameSettings.snappingStep.Increment();

        else if (ShortcutInput.GetInputDown(Shortcut.StepDecrease))
            GameSettings.snappingStep.Decrement();

        else if (ShortcutInput.GetInputDown(Shortcut.Delete) && editor.currentSelectedObjects.Count > 0)
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
                if (applicationMode != ApplicationMode.Playing)
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
            HighlightAll();
        }
        else if (ShortcutInput.GetInputDown(Shortcut.SelectAllSection))
        {
            services.toolpanelController.SetCursor();
            HighlightCurrentSection();
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            bool success = false;

            if (ShortcutInput.GetInputDown(Shortcut.ActionHistoryUndo))
            {
                if (!editor.commandStack.isAtStart)
                {
                    editor.commandStack.Pop();
                    success = true;
                }
                //success = editor.actionHistory.Undo(editor);
            }
            else if (ShortcutInput.GetInputDown(Shortcut.ActionHistoryRedo))
            {
                if (!editor.commandStack.isAtEnd)
                {
                    editor.commandStack.Push();
                    success = true;
                }
                //success = editor.actionHistory.Redo(editor);
            }

            if (success)
            {
                EventSystem.current.SetSelectedGameObject(null);
                groupSelect.reset();
                TimelineHandler.externalUpdate = true;
            }
        }

        if (editor.currentSelectedObjects.Count > 0)
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

    void HighlightAll()
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

    delegate void SongObjectSelectedManipFn(System.Collections.Generic.IEnumerable<SongObject> songObjects);
    void HighlightCurrentSection()
    {
        editor.currentSelectedObject = null;

        AddHighlightCurrentSection();
    }

    public void AddHighlightCurrentSection(int sectionOffset = 0)
    {
        HighlightCurrentSection(editor.AddToSelectedObjects, sectionOffset);
    }

    public void RemoveHighlightCurrentSection(int sectionOffset = 0)
    {
        HighlightCurrentSection(editor.RemoveFromSelectedObjects, sectionOffset);
    }

    public void AddOrRemoveHighlightCurrentSection()
    {
        HighlightCurrentSection(editor.AddOrRemoveSelectedObjects);
    }

    void HighlightCurrentSection(SongObjectSelectedManipFn manipFn, int sectionOffset = 0)
    {
        // Get the previous and next section
        uint currentPos = editor.currentTickPos;
        var sections = editor.currentSong.sections;
        int maxSectionIndex = 0;
        while (maxSectionIndex < sections.Count && !(sections[maxSectionIndex].tick > currentPos))
        {
            ++maxSectionIndex;
        }

        maxSectionIndex += sectionOffset;

        uint rangeMin = (maxSectionIndex - 1) >= 0 ? sections[maxSectionIndex - 1].tick : 0;
        uint rangeMax = maxSectionIndex < sections.Count ? sections[maxSectionIndex].tick : uint.MaxValue;
        if (rangeMax > 0)
            --rangeMax;

        if (viewMode == ViewMode.Chart)
        {
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentChart.chartObjects, rangeMin, rangeMax));
        }
        else
        {
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentSong.syncTrack, rangeMin, rangeMax));
            manipFn(SongObjectHelper.GetRangeCopy(editor.currentSong.eventsAndSections, rangeMin, rangeMax));
        }
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
