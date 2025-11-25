// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToolPanelController : MonoBehaviour {
    ChartEditor editor;
    public Toggle viewModeToggle;
    public KeysNotePlacementModePanelController keysModePanel;
    public MouseNoteTypePanelController mouseModePanel;

    [SerializeField]
    Button cursorSelect = null;
    [SerializeField]
    Button eraserSelect = null;
    [SerializeField]
    Button noteSelect = null;
    [SerializeField]
    Button starpowerSelect = null;
    [SerializeField]
    Button bpmSelect = null;
    [SerializeField]
    Button timeSignatureSelect = null;
    [SerializeField]
    Button sectionSelect = null;
    [SerializeField]
    Button eventSelect = null;
    [SerializeField]
    Button drumRollSelect = null;

    [SerializeField]
    Sprite globalEventSprite = null;
    Sprite localEventSprite;
    Image eventImage;

    void Start()
    {
        editor = ChartEditor.Instance;

        eventImage = eventSelect.GetComponent<Image>();
        localEventSprite = eventImage.sprite;

        editor.events.viewModeSwitchEvent.Register(OnViewModeSwitch);
        editor.events.toolChangedEvent.Register(RefreshAvailableTools);
        editor.events.chartReloadedEvent.Register(RefreshAvailableTools);

        RefreshAvailableTools();
    }

    // Update is called once per frame
    void Update () {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleViewMode) && (editor.currentState == ChartEditor.State.Editor || editor.currentState == ChartEditor.State.Playing))
        {
            viewModeToggle.isOn = !viewModeToggle.isOn;
        }

        keysModePanel.gameObject.SetActive(editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Note && Globals.gameSettings.keysModeEnabled);
        mouseModePanel.gameObject.SetActive(editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Note && !Globals.gameSettings.keysModeEnabled);

        Shortcuts();
    }

    void Shortcuts()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectCursor))
            cursorSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectEraser))
            eraserSelect.onClick.Invoke();

        // else if (Input.GetKeyDown(KeyCode.L))
        // groupSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectNote))
            noteSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectStarpower))
            starpowerSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectBpm))
            bpmSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectTimeSignature))
            timeSignatureSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectSection))
            sectionSelect.onClick.Invoke();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectEvent))
            eventSelect.onClick.Invoke();

        else if (drumRollSelect.isActiveAndEnabled && MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToolSelectDrumRoll))
            drumRollSelect.onClick.Invoke();
    }

    public void ToggleSongViewMode(bool globalView)
    {
        bool globalViewActive = Globals.viewMode == Globals.ViewMode.Song;

        if (globalViewActive != globalView)
            ChartEditor.Instance.globals.ToggleSongViewMode(globalView);
    }

    void OnViewModeSwitch(in Globals.ViewMode viewMode)
    {
        if (viewMode == Globals.ViewMode.Chart)
            eventImage.sprite = localEventSprite;
        else if (viewMode == Globals.ViewMode.Song)
            eventImage.sprite = globalEventSprite;

        bool globalView = viewMode == Globals.ViewMode.Song;
        if (viewModeToggle.isOn != globalView)  // Setting this when it's the same will just call the onclick events again
        {
            viewModeToggle.isOn = globalView;
        }
    }

    void RefreshAvailableTools()
    {
        drumRollSelect.gameObject.SetActive(Globals.drumMode);
    }
}
