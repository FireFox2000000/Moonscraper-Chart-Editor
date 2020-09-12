// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.EventSystems;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

/// <summary>
/// Loads and processes any persistent data, like save data, as well as just convenient defines and properties that exists outside of the "Chart Editor" concept.
/// </summary>
public class Globals : MonoBehaviour {
    public static readonly string applicationBranchName = "";

    public static readonly string LINE_ENDING = "\r\n";
    public const string TABSPACE = "  ";
    public static string autosaveLocation;
    static string workingDirectory = string.Empty;
    public static string realWorkingDirectory { get { return workingDirectory; } }

    public static readonly string[] validAudioExtensions = { ".ogg", ".wav", ".mp3" };
    public static readonly string[] validTextureExtensions = { ".jpg", ".png" };
    public static string[] localEvents = { };
    public static string[] globalEvents = { };

    public static GameSettings gameSettings { get; private set; }

    [Header("Misc.")]
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
    public Services services
    {
        get
        {
            if (_services == null)
                _services = GetComponent<Services>();

            return _services;
        }
    }

    void Awake()
    {
        gameSettings = new GameSettings();

        Application.runInBackground = true;

        autosaveLocation = Application.persistentDataPath + "/autosave.chart";

        viewMode = ViewMode.Chart;
        editor = ChartEditor.Instance;

#if !UNITY_EDITOR
        workingDirectory = DirectoryHelper.GetMainDirectory();
#else
        workingDirectory = Application.dataPath;
#endif
        // Bass init
        string audioInitErr = string.Empty;
        if (!AudioManager.Init(out audioInitErr))
        {
            editor.errorManager.QueueErrorMessage(audioInitErr);
        }

        LoadGameSettings();
        Localiser.LocaliseScene();

        localEvents = LoadCommonEvents("local_events.txt");
        globalEvents = LoadCommonEvents("global_events.txt");

        HintMouseOver.style = hintMouseOverStyle; 
    }

    void Start()
    {
        StartCoroutine(AutosaveCheck());
    }

    public static void LoadExternalControls(MSChartEditorInput.MSChartEditorActionContainer controls)
    {
        gameSettings = new GameSettings();
        gameSettings.controls = controls;
    }

    void LoadGameSettings()
    {
        gameSettings.Load(GetConfigPath(), GetInputBindingsPath());

        // Check for valid fps values
        int fps = gameSettings.targetFramerate;
        if (fps != 60 && fps != 120 && fps != 240)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = fps;

        AudioListener.volume = gameSettings.vol_master;
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

            NativeMessageBox.Result result = NativeMessageBox.Show(autosaveText, autosaveCaption, NativeMessageBox.Type.YesNo, editor.windowHandleManager.nativeWindow);

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

        var currentTool = editor.toolManager.currentToolId;
        snapLockWarning.gameObject.SetActive(Globals.gameSettings.keysModeEnabled && currentTool != EditorObjectToolManager.ToolID.Cursor && currentTool != EditorObjectToolManager.ToolID.Eraser);

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
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleMetronome))
        {
            services.ToggleMetronome();
            services.notificationBar.PushNotification("METRONOME TOGGLED " + Services.BoolToStrOnOff(Globals.gameSettings.metronomeActive), 2, true);
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.FileSave))
            editor._Save();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.FileSaveAs))
            editor.SaveAs();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.FileLoad))
            editor.Load();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.FileNew))
            editor.New();
    }

    public void Quit()
    {
        Globals.gameSettings.targetFramerate = Application.targetFrameRate;
        Globals.gameSettings.Save(GetConfigPath(), GetInputBindingsPath());

        // Delete autosaved chart. If chart is not deleted then that means there may have been a problem like a crash and the autosave should be reloaded the next time the program is opened. 
        if (File.Exists(autosaveLocation))
            File.Delete(autosaveLocation);
    }

    string GetConfigPath()
    {
        return Application.persistentDataPath + "/config.ini";
    }

    string GetInputBindingsPath()
    {
        return Application.persistentDataPath + "/controls.json";
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

    public void ToggleSongViewMode(bool globalView)
    {
        ViewMode originalView = viewMode;

        if (globalView)
        {
            viewMode = ViewMode.Song;
        }
        else
        {
            viewMode = ViewMode.Chart;
        }

        ChartEditor editor = ChartEditor.Instance;
        if (editor.toolManager.currentToolId != EditorObjectToolManager.ToolID.Note)        // Allows the note panel to pop up instantly
            editor.selectedObjectsManager.currentSelectedObject = null;

        editor.events.viewModeSwitchEvent.Fire(viewMode);
    }
}
