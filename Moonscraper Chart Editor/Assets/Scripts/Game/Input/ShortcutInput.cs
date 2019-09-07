using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE.Input;

public enum Shortcut
{
    ActionHistoryRedo,
    ActionHistoryUndo,

    AddSongObject,

    BpmIncrease,
    BpmDecrease,

    ChordSelect,

    ClipboardCopy,
    ClipboardCut,
    ClipboardPaste,

    Delete, 

    FileLoad,
    FileNew,
    FileSave,
    FileSaveAs,

    MoveStepPositive,
    MoveStepNegative,
    MoveMeasurePositive,
    MoveMeasureNegative,

    NoteSetNatural,
    NoteSetStrum,
    NoteSetHopo,
    NoteSetTap,
    
    PlayPause,

    SelectAll,
    SelectAllSection,
    StepDecrease,
    StepIncrease,

    SectionJumpPositive,
    SectionJumpNegative,
    SectionJumpMouseScroll,

    ToggleBpmAnchor,
    ToggleClap,
    ToggleExtendedSustains,
    ToggleMetronome,
    ToggleMouseMode,
    ToggleNoteForced,
    ToggleNoteTap,
    ToggleViewMode, 
    
    ToolNoteBurst,
    ToolNoteHold,
    ToolSelectCursor,
    ToolSelectEraser,
    ToolSelectNote,
    ToolSelectStarpower,
    ToolSelectBpm,
    ToolSelectTimeSignature,
    ToolSelectSection,
    ToolSelectEvent,
}

public static class ShortcutInput
{
    static Dictionary<Shortcut, bool> inputIsNotRebindable = new Dictionary<Shortcut, bool>()
    {
        { Shortcut.ActionHistoryRedo, true },
        { Shortcut.ActionHistoryUndo, true },
        { Shortcut.ChordSelect, true },
        { Shortcut.ClipboardCopy, true },
        { Shortcut.ClipboardCut, true },
        { Shortcut.ClipboardPaste, true },
        { Shortcut.Delete, true },
        { Shortcut.FileLoad, true },
        { Shortcut.FileNew, true },
        { Shortcut.FileSave, true },
        { Shortcut.FileSaveAs, true },
        { Shortcut.PlayPause, true },
        { Shortcut.SectionJumpMouseScroll, true },
    };

    public class ShortcutLUT
    {
        InputAction[] actionConfigs;

        public ShortcutLUT()
        {
            actionConfigs = new InputAction[System.Enum.GetValues(typeof(Shortcut)).Length];

            for (int i = 0; i < actionConfigs.Length; ++i)
            {
                bool notRebindable;
                if (!inputIsNotRebindable.TryGetValue((Shortcut)i, out notRebindable))
                {
                    notRebindable = false;
                }

                actionConfigs[i] = new InputAction(!notRebindable);
            }
        }

        public void Insert(Shortcut key, InputAction entry)
        {
            actionConfigs[(int)key] = entry;
        }

        public InputAction GetActionConfig(Shortcut key)
        {
            return actionConfigs[(int)key];
        }
    }

    public static ShortcutLUT allInputs { get; private set; }

    static ShortcutInput()
    {
        allInputs = new ShortcutLUT();

        LoadDefaultControls();
    }

    static void LoadDefaultControls()
    {
        {
            allInputs.GetActionConfig(Shortcut.AddSongObject).kbMaps[0] = new KeyboardMap() { KeyCode.Alpha1 };
            allInputs.GetActionConfig(Shortcut.BpmIncrease).kbMaps[0] = new KeyboardMap() { KeyCode.Equals, };
            allInputs.GetActionConfig(Shortcut.BpmDecrease).kbMaps[0] = new KeyboardMap() { KeyCode.Minus, };
            allInputs.GetActionConfig(Shortcut.Delete).kbMaps[0] = new KeyboardMap() { KeyCode.Delete };
            allInputs.GetActionConfig(Shortcut.PlayPause).kbMaps[0] = new KeyboardMap() { KeyCode.Space };
            allInputs.GetActionConfig(Shortcut.MoveStepPositive).kbMaps[0] = new KeyboardMap() { KeyCode.UpArrow };
            allInputs.GetActionConfig(Shortcut.MoveStepNegative).kbMaps[0] = new KeyboardMap() { KeyCode.DownArrow };
            allInputs.GetActionConfig(Shortcut.MoveMeasurePositive).kbMaps[0] = new KeyboardMap() { KeyCode.PageUp };
            allInputs.GetActionConfig(Shortcut.MoveMeasureNegative).kbMaps[0] = new KeyboardMap() { KeyCode.PageDown };
            allInputs.GetActionConfig(Shortcut.NoteSetNatural).kbMaps[0] = new KeyboardMap() { KeyCode.X };
            allInputs.GetActionConfig(Shortcut.NoteSetStrum).kbMaps[0] = new KeyboardMap() { KeyCode.S };
            allInputs.GetActionConfig(Shortcut.NoteSetHopo).kbMaps[0] = new KeyboardMap() { KeyCode.H };
            allInputs.GetActionConfig(Shortcut.NoteSetTap).kbMaps[0] = new KeyboardMap() { KeyCode.T };
            allInputs.GetActionConfig(Shortcut.StepIncrease).kbMaps[0] = new KeyboardMap() { KeyCode.W };
            allInputs.GetActionConfig(Shortcut.StepIncrease).kbMaps[1] = new KeyboardMap() { KeyCode.RightArrow };
            allInputs.GetActionConfig(Shortcut.StepDecrease).kbMaps[0] = new KeyboardMap() { KeyCode.Q };
            allInputs.GetActionConfig(Shortcut.StepDecrease).kbMaps[1] = new KeyboardMap() { KeyCode.LeftArrow };
            allInputs.GetActionConfig(Shortcut.ToggleBpmAnchor).kbMaps[0] = new KeyboardMap() { KeyCode.A };
            allInputs.GetActionConfig(Shortcut.ToggleClap).kbMaps[0] = new KeyboardMap() { KeyCode.N };
            allInputs.GetActionConfig(Shortcut.ToggleExtendedSustains).kbMaps[0] = new KeyboardMap() { KeyCode.E };
            allInputs.GetActionConfig(Shortcut.ToggleMetronome).kbMaps[0] = new KeyboardMap() { KeyCode.M };
            allInputs.GetActionConfig(Shortcut.ToggleMouseMode).kbMaps[0] = new KeyboardMap() { KeyCode.BackQuote };
            allInputs.GetActionConfig(Shortcut.ToggleNoteForced).kbMaps[0] = new KeyboardMap() { KeyCode.F };
            allInputs.GetActionConfig(Shortcut.ToggleNoteTap).kbMaps[0] = new KeyboardMap() { KeyCode.T };
            allInputs.GetActionConfig(Shortcut.ToggleViewMode).kbMaps[0] = new KeyboardMap() { KeyCode.G };
            allInputs.GetActionConfig(Shortcut.ToolNoteBurst).kbMaps[0] = new KeyboardMap() { KeyCode.B };
            allInputs.GetActionConfig(Shortcut.ToolNoteHold).kbMaps[0] = new KeyboardMap() { KeyCode.H };
            allInputs.GetActionConfig(Shortcut.ToolSelectCursor).kbMaps[0] = new KeyboardMap() { KeyCode.J };
            allInputs.GetActionConfig(Shortcut.ToolSelectEraser).kbMaps[0] = new KeyboardMap() { KeyCode.K };
            allInputs.GetActionConfig(Shortcut.ToolSelectNote).kbMaps[0] = new KeyboardMap() { KeyCode.Y };
            allInputs.GetActionConfig(Shortcut.ToolSelectStarpower).kbMaps[0] = new KeyboardMap() { KeyCode.U };
            allInputs.GetActionConfig(Shortcut.ToolSelectBpm).kbMaps[0] = new KeyboardMap() { KeyCode.I };
            allInputs.GetActionConfig(Shortcut.ToolSelectTimeSignature).kbMaps[0] = new KeyboardMap() { KeyCode.O };
            allInputs.GetActionConfig(Shortcut.ToolSelectSection).kbMaps[0] = new KeyboardMap() { KeyCode.P };
            allInputs.GetActionConfig(Shortcut.ToolSelectEvent).kbMaps[0] = new KeyboardMap() { KeyCode.L };
        }

        {
            KeyboardMap.ModifierKeys modiInput = KeyboardMap.ModifierKeys.Ctrl;
            allInputs.GetActionConfig(Shortcut.ClipboardCopy).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.C };
            allInputs.GetActionConfig(Shortcut.ClipboardCut).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.X };
            allInputs.GetActionConfig(Shortcut.ClipboardPaste).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.V };
            allInputs.GetActionConfig(Shortcut.FileLoad).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.O };
            allInputs.GetActionConfig(Shortcut.FileNew).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.N };
            allInputs.GetActionConfig(Shortcut.FileSave).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.S };
            allInputs.GetActionConfig(Shortcut.ActionHistoryRedo).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.Y };
            allInputs.GetActionConfig(Shortcut.ActionHistoryUndo).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.Z };
            allInputs.GetActionConfig(Shortcut.SelectAll).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.A };
        }

        {
            KeyboardMap.ModifierKeys modiInput = KeyboardMap.ModifierKeys.Shift;

            allInputs.GetActionConfig(Shortcut.ChordSelect).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.LeftShift };
            allInputs.GetActionConfig(Shortcut.ChordSelect).kbMaps[1] = new KeyboardMap(modiInput) { KeyCode.RightShift };
        }

        {
            KeyboardMap.ModifierKeys modiInput = KeyboardMap.ModifierKeys.Ctrl | KeyboardMap.ModifierKeys.Shift;

            allInputs.GetActionConfig(Shortcut.FileSaveAs).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.S };
            allInputs.GetActionConfig(Shortcut.ActionHistoryRedo).kbMaps[1] = new KeyboardMap(modiInput) { KeyCode.Z };
        }

        {
            KeyboardMap.ModifierKeys modiInput = KeyboardMap.ModifierKeys.Alt;

            allInputs.GetActionConfig(Shortcut.SectionJumpPositive).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.UpArrow };
            allInputs.GetActionConfig(Shortcut.SectionJumpNegative).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.DownArrow };
            allInputs.GetActionConfig(Shortcut.SelectAllSection).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.A };
            allInputs.GetActionConfig(Shortcut.SectionJumpMouseScroll).kbMaps[0] = new KeyboardMap(modiInput) { KeyCode.LeftAlt };
            allInputs.GetActionConfig(Shortcut.SectionJumpMouseScroll).kbMaps[1] = new KeyboardMap(modiInput) { KeyCode.RightAlt };
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static bool modifierInput { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }
    public static bool secondaryInput { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    public static bool alternativeInput { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }

    public static bool GetInputDown(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return allInputs.GetActionConfig(key).GetInputDown();
        }

        return false;
    }

    public static bool GetInputUp(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return allInputs.GetActionConfig(key).GetInputUp();
        }

        return false;
    }

    public static bool GetInput(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return allInputs.GetActionConfig(key).GetInput();
        }

        return false;
    }

    public static bool GetGroupInputDown(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInputDown(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInputUp(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (GetInput(key))
                return true;
        }

        return false;
    }
}
