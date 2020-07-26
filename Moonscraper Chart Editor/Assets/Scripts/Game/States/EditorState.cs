// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MoonscraperEngine;

public class EditorState : SystemManagerState
{
    public override void Update()
    {
        base.Update();

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
