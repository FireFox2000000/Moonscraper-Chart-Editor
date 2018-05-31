// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToolPanelController : MonoBehaviour {
    ChartEditor editor;
    public Toggle viewModeToggle;
    public KeysNotePlacementModePanelController keysModePanel;

    [SerializeField]
    Button cursorSelect;
    [SerializeField]
    Button eraserSelect;
    [SerializeField]
    Button noteSelect;
    [SerializeField]
    Button starpowerSelect;
    [SerializeField]
    Button bpmSelect;
    [SerializeField]
    Button timeSignatureSelect;
    [SerializeField]
    Button sectionSelect;
    [SerializeField]
    Button eventSelect;

    [SerializeField]
    Sprite globalEventSprite;
    Sprite localEventSprite;
    Image eventImage;

    void Start()
    {
        editor = ChartEditor.GetInstance();

        eventImage = eventSelect.GetComponent<Image>();
        localEventSprite = eventImage.sprite;

        TriggerManager.onViewModeSwitchTriggerList.Add(OnViewModeSwitch);
    }

    // Update is called once per frame
    void Update () {
        if (ShortcutInput.GetInputDown(Shortcut.ToggleViewMode) && (Globals.applicationMode == Globals.ApplicationMode.Editor || Globals.applicationMode == Globals.ApplicationMode.Playing))
        {
            viewModeToggle.isOn = !viewModeToggle.isOn;
        }

        keysModePanel.gameObject.SetActive(Toolpane.currentTool == Toolpane.Tools.Note && GameSettings.keysModeEnabled);

        Shortcuts();
    }

    void Shortcuts()
    {
        if (ShortcutInput.GetInputDown(Shortcut.ToolSelectCursor))
            SetCursor();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectEraser))
            eraserSelect.onClick.Invoke();

        // else if (Input.GetKeyDown(KeyCode.L))
        // groupSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectNote))
            noteSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectStarpower))
            starpowerSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectBpm))
            bpmSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectTimeSignature))
            timeSignatureSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectSection))
            sectionSelect.onClick.Invoke();

        else if (ShortcutInput.GetInputDown(Shortcut.ToolSelectEvent))
            eventSelect.onClick.Invoke();
    }

    public void ToggleSongViewMode(bool globalView)
    {
        Globals.ViewMode originalView = Globals.viewMode;

        if (globalView)
        {
            Globals.viewMode = Globals.ViewMode.Song;

            if (Toolpane.currentTool == Toolpane.Tools.Note || Toolpane.currentTool == Toolpane.Tools.Starpower || Toolpane.currentTool == Toolpane.Tools.ChartEvent || Toolpane.currentTool == Toolpane.Tools.GroupSelect)
            {
                cursorSelect.onClick.Invoke();
            }
        }
        else
        {
            Globals.viewMode = Globals.ViewMode.Chart;

            if (Toolpane.currentTool == Toolpane.Tools.BPM || Toolpane.currentTool == Toolpane.Tools.Timesignature || Toolpane.currentTool == Toolpane.Tools.Section)
            {
                cursorSelect.onClick.Invoke();
            }
        }

        if (viewModeToggle.isOn != globalView)
        {
            viewModeToggle.isOn = globalView;
        }

        if (Toolpane.currentTool != Toolpane.Tools.Note)        // Allows the note panel to pop up instantly
            editor.currentSelectedObject = null;

        TriggerManager.FireViewModeSwitchTriggers();
    }

    void OnViewModeSwitch(Globals.ViewMode viewMode)
    {
        if (viewMode == Globals.ViewMode.Chart)
            eventImage.sprite = localEventSprite;
        else if (viewMode == Globals.ViewMode.Song)
                eventImage.sprite = globalEventSprite;
    }

    public void SetCursor()
    {
        cursorSelect.onClick.Invoke();
    }
}
