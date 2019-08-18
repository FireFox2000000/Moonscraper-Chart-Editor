using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorState : SystemManagerState
{
    public override void Update()
    {
        base.Update();

        ChartEditor editor = ChartEditor.Instance;
        Services services = editor.services;
        Globals.ViewMode viewMode = Globals.viewMode;

        if (services.CanPlay())
        {
            if (ShortcutInput.GetInputDown(Shortcut.PlayPause))
            {
                editor.Play();
                return;
            }
            else if (editor.inputManager.mainGamepad.GetButtonPressed(GamepadInput.Button.Start))
            {
                editor.StartGameplay();
                return;
            }
        }

        else if (ShortcutInput.GetInputDown(Shortcut.StepIncrease))
            GameSettings.snappingStep.Increment();

        else if (ShortcutInput.GetInputDown(Shortcut.StepDecrease))
            GameSettings.snappingStep.Decrement();

        else if (ShortcutInput.GetInputDown(Shortcut.Delete) && editor.selectedObjectsManager.currentSelectedObjects.Count > 0)
            editor.Delete();

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
                editor.groupSelect.reset();
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
}
