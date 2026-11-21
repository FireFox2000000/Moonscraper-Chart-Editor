// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MoonscraperEngine;

public class EditorState : SystemManagerState
{
    // Whether a dropdown's list or a menu bar menu was open when this state last ran. The Esc press that closes one is
    // only for the dropdown: the event system can close a list before this state runs, and the menu bar closes its menu
    // after it.
    static bool s_dropdownWasOpen;

    // Whether a menu bar menu is open, or a dropdown's list: the list's current item takes the UI selection when it opens,
    // and the dropdown takes it back when it closes.
    static bool DropdownOpen()
    {
        if (UnityEngine.UI.CustomUnityDropdown.anyExpanded)
            return true;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
            return false;

        UnityEngine.UI.Dropdown dropdown = selected.GetComponentInParent<UnityEngine.UI.Dropdown>();
        return dropdown != null && dropdown.gameObject != selected;
    }

    public override void Update()
    {
        base.Update();

        bool dropdownOpen = DropdownOpen();
        bool escForDropdown = dropdownOpen || s_dropdownWasOpen;
        s_dropdownWasOpen = dropdownOpen;

        ChartEditor editor = ChartEditor.Instance;
        Services services = editor.services;
        Globals.ViewMode viewMode = Globals.viewMode;

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepIncrease))
            Globals.gameSettings.snappingStep.Increment();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepDecrease))
            Globals.gameSettings.snappingStep.Decrement();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepIncreaseBy1))
            Globals.gameSettings.snappingStep.AdjustBy(1);

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StepDecreaseBy1))
            Globals.gameSettings.snappingStep.AdjustBy(-1);

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ReloadAudio))
        {
            editor.ReloadAudio();
        }

        if (editor.groupMove.movementInProgress)
            return;

        if (services.CanPlay())
        {
            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.PlayPause))
            {
                editor.Play();
                return;
            }
            else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.StartGameplay))
            {
                editor.StartGameplay();
                return;
            }
        }

        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.Delete) && editor.selectedObjectsManager.currentSelectedObjects.Count > 0)
            editor.Delete();

        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.SelectAll))
        {
            editor.toolManager.ChangeTool(EditorObjectToolManager.ToolID.Cursor);
            editor.selectedObjectsManager.SelectAllInView(viewMode);
        }
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.SelectAllSection))
        {
            editor.toolManager.ChangeTool(EditorObjectToolManager.ToolID.Cursor);
            editor.selectedObjectsManager.HighlightCurrentSection(viewMode);
        }
        else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.DeselectAll) && !escForDropdown)
        {
            editor.selectedObjectsManager.currentSelectedObject = null;
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            bool success = false;

            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ActionHistoryUndo))
            {
                if (!editor.commandStack.isAtStart && editor.services.CanUndo())
                {
                    editor.UndoWrapper();
                    success = true;
                }
            }
            else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ActionHistoryRedo))
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
                editor.groupSelect.reset();
                TimelineHandler.Repaint();
            }
        }

        if (editor.selectedObjectsManager.currentSelectedObjects.Count > 0)
        {
            if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ClipboardCut))
                editor.Cut();
            else if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ClipboardCopy))
                editor.Copy();
        }
    }
}
