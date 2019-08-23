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
        editor = ChartEditor.Instance;

        eventImage = eventSelect.GetComponent<Image>();
        localEventSprite = eventImage.sprite;

        editor.events.viewModeSwitchEvent.Register(OnViewModeSwitch);
    }

    // Update is called once per frame
    void Update () {
        if (ShortcutInput.GetInputDown(Shortcut.ToggleViewMode) && (editor.currentState == ChartEditor.State.Editor || editor.currentState == ChartEditor.State.Playing))
        {
            viewModeToggle.isOn = !viewModeToggle.isOn;
        }

        keysModePanel.gameObject.SetActive(editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Note && GameSettings.keysModeEnabled);

        Shortcuts();
    }

    void Shortcuts()
    {
        if (ShortcutInput.GetInputDown(Shortcut.ToolSelectCursor))
            cursorSelect.onClick.Invoke();

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
}
