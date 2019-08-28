using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class UpdateableService : MonoBehaviour
{
    public abstract void OnServiceUpdate();

    protected virtual void Start()
    {
        ChartEditor.Instance.globals.services.RegisterUpdateableService(this);
    }
}

public class Services : MonoBehaviour
{
    [Header("Area range")]
    public RectTransform area;

    [Header("UI Services")]
    public DropdownNotification notificationBar;
    public UIServices uiServices;

    [HideInInspector]
    public LoadingTasksManager loadingTasksManager { get; private set; }

    Rect toolScreenArea;
    public static bool IsInDropDown = false;
    static Vector2 prevScreenSize;
    public static bool IsTyping = false;

    public MouseMonitor mouseMonitorSystem { get; private set; }
    List<UpdateableService> updateableServices = new List<UpdateableService>();

    public static bool HasScreenResized
    {
        get
        {
            return (prevScreenSize.x != Screen.width || prevScreenSize.y != Screen.height);
        }
    }

    public bool InToolArea
    {
        get
        {
            Vector2 mousePosition = uiServices.GetUIMousePosition();

            if (mousePosition.x < toolScreenArea.xMin ||
                    mousePosition.x > toolScreenArea.xMax ||
                    mousePosition.y < toolScreenArea.yMin ||
                    mousePosition.y > toolScreenArea.yMax)
                return false;
            else
                return true;
        }
    }

    public void OnScreenResize()
    {
        toolScreenArea = area.GetScreenCorners();
    }

    static bool _IsInDropDown
    {
        get
        {
            MouseMonitor mouseMonitorSystem = ChartEditor.Instance.services.mouseMonitorSystem;
            GameObject currentUIUnderPointer = mouseMonitorSystem.GetUIRaycastableUnderPointer();
            if (currentUIUnderPointer != null && (currentUIUnderPointer.GetComponentInChildren<ScrollRect>() || currentUIUnderPointer.GetComponentInParent<ScrollRect>()))
                return true;

            if ((EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponentInParent<Dropdown>() == null) && !mouseMonitorSystem.GetUIUnderPointer<Dropdown>())
            {
                return false;
            }
            else
                return true;
        }
    }

    static bool _IsTyping
    {
        get
        {
            if (EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                return false;
            else
                return true;
        }
    }

    public void ResetAspectRatio()
    {
        int height = Screen.height;
        int width = (int)(16.0f / 9.0f * height);

        Screen.SetResolution(width, height, false);
    }

    public static string BoolToStrOnOff(bool val)
    {
        string toggleStr = val ? "ON" : "OFF";

        return toggleStr;
    }

    public void ToggleMouseLockMode()
    {
        SetKeysMode(!GameSettings.keysModeEnabled);
        Debug.Log("Keys mode toggled " + GameSettings.keysModeEnabled);
    }

    public void ToggleExtendedSustains()
    {
        GameSettings.extendedSustainsEnabled = !GameSettings.extendedSustainsEnabled;
        Debug.Log("Extended sustains toggled " + GameSettings.extendedSustainsEnabled);
    }

    public void ToggleMetronome()
    {
        GameSettings.metronomeActive = !GameSettings.metronomeActive;
        Debug.Log("Metronome toggled " + GameSettings.metronomeActive);
    }

    public void SetKeysMode(bool enabled)
    {
        GameSettings.keysModeEnabled = enabled;
        ChartEditor.Instance.events.keyboardModeToggledEvent.Fire(GameSettings.keysModeEnabled);
    }

    public bool CanUndo()
    {
        ChartEditor editor = ChartEditor.Instance;
        return !editor.commandStack.isAtStart && !editor.groupMove.movementInProgress;
    }

    public bool CanRedo()
    {
        ChartEditor editor = ChartEditor.Instance;
        return !editor.commandStack.isAtEnd && !editor.groupMove.movementInProgress;
    }

    public bool CanPlay()
    {
        return !ChartEditor.Instance.groupMove.movementInProgress;
    }

    public float songAudioCalibrationTimeOffset
    {
        get
        {
            return GameSettings.audioCalibrationMS / 1000.0f * GameSettings.gameSpeed;
        }
    }

    public float totalSongAudioOffset
    {
        get
        {
            return ChartEditor.Instance.currentSong.offset + songAudioCalibrationTimeOffset;
        }
    }

    public float currentAudioTime
    {
        get
        {
            return ChartEditor.Instance.currentVisibleTime + totalSongAudioOffset;
        }
    }

    public float sfxCalibratedStrikelinePos
    {
        get
        {
            ChartEditor editor = ChartEditor.Instance;
            Vector3 strikelinePos = editor.visibleStrikeline.position;
            float posCalibrationOffset = TickFunctions.TimeToWorldYPosition(GameSettings.audioCalibrationMS / 1000.0f) * GameSettings.gameSpeed;
            return strikelinePos.y + posCalibrationOffset;
        }
    }

    public float sfxAudioTime
    {
        get
        {
            ChartEditor editor = ChartEditor.Instance;
            Song currentSong = editor.currentSong;
            AudioStream mainAudio = currentSong.mainSongAudio;

            float currentAudioTime = 0;
            if (mainAudio != null && mainAudio.IsPlaying())
            {
                currentAudioTime = mainAudio.CurrentPositionInSeconds();
            }
            else
            {
                float audioStrikelinePos = editor.services.sfxCalibratedStrikelinePos;
                currentAudioTime = TickFunctions.WorldYPositionToTime(audioStrikelinePos);
            }

            return currentAudioTime + GameSettings.clapCalibrationMS / 1000.0f;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    
    private void Awake()
    {
        loadingTasksManager = GetComponent<LoadingTasksManager>();

        mouseMonitorSystem = new MouseMonitor();
        ChartEditor.Instance.RegisterPersistentSystem(ChartEditor.State.Editor, mouseMonitorSystem);
    }

    // Use this for initialization
    void Start()
    {
        toolScreenArea = area.GetScreenCorners();
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
    }

    public void RegisterUpdateableService(UpdateableService service)
    {
        updateableServices.Add(service);
    }

    // Update is called once per frame
    void Update()
    {
        IsInDropDown = _IsInDropDown;
        IsTyping = _IsTyping;

        if (HasScreenResized)
            OnScreenResize();

        foreach(UpdateableService service in updateableServices)
        {
            service.OnServiceUpdate();
        }
    }

    void LateUpdate()
    {
        prevScreenSize.x = Screen.width;
        prevScreenSize.y = Screen.height;
    }
}
