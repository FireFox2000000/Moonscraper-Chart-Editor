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

    [System.Serializable]
    public class ShortcutActionContainer
    {
        [System.Serializable]
        struct InputSaveData
        {
            public string action;
            public InputAction input;
        }

        [SerializeField]
        List<InputSaveData> saveData = new List<InputSaveData>();    // Safer save data format, to handle cases where the Shortcut enum list may get updated or values are shifted around
        InputAction[] actionConfigCleanLookup;

        public ShortcutActionContainer()
        {
            actionConfigCleanLookup = new InputAction[System.Enum.GetValues(typeof(Shortcut)).Length];

            for (int i = 0; i < actionConfigCleanLookup.Length; ++i)
            {
                bool notRebindable;
                if (!inputIsNotRebindable.TryGetValue((Shortcut)i, out notRebindable))
                {
                    notRebindable = false;
                }

                actionConfigCleanLookup[i] = new InputAction(!notRebindable);
            }
        }

        public void Insert(Shortcut key, InputAction entry)
        {
            actionConfigCleanLookup[(int)key] = entry;
        }

        public InputAction GetActionConfig(Shortcut key)
        {
            return actionConfigCleanLookup[(int)key];
        }

        public void LoadFromSaveData()
        {
            foreach(var keyVal in saveData)
            {
                Shortcut enumVal;
                if (System.Enum.TryParse(keyVal.action, out enumVal))
                {
                    actionConfigCleanLookup[(int)enumVal] = keyVal.input;

                    bool notRebindable;
                    if (!inputIsNotRebindable.TryGetValue(enumVal, out notRebindable))
                    {
                        notRebindable = false;
                    }

                    actionConfigCleanLookup[(int)enumVal].rebindable = !notRebindable;
                }
                else
                {
                    Debug.LogError("Unable to parse " + keyVal.action + " as an input action");
                }
            }
        }

        public void UpdateSaveData()
        {
            saveData.Clear();
            for (int i = 0; i < actionConfigCleanLookup.Length; ++i)
            {
                Shortcut sc = (Shortcut)i;

                bool notRebindable;
                if (!inputIsNotRebindable.TryGetValue(sc, out notRebindable))
                {
                    notRebindable = false;
                }

                if (notRebindable)
                    continue;

                var newItem = new InputSaveData();
                newItem.action = sc.ToString();
                newItem.input = actionConfigCleanLookup[i];

                saveData.Add(newItem);

            }
        }
    }

    static ShortcutActionContainer primaryInputs { get { return GameSettings.controls; } }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static bool modifierInput { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }
    public static bool secondaryInput { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    public static bool alternativeInput { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }

    public static bool GetInputDown(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown();
        }

        return false;
    }

    public static bool GetInputUp(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp();
        }

        return false;
    }

    public static bool GetInput(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return primaryInputs.GetActionConfig(key).GetInput();
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
