using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE;
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
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public static readonly int Global = 0;
        public static readonly int KeyboardMode = 1;
        public static readonly int ToolNoteCursor = 2;
        public static readonly int ToolNoteKeyboard = 3;

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(4);

        static Category()
        {
            interactionMatrix.SetInteractableAll(Global);
        }
    }

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = true, hiddenInLists = false, category = Category.Global };

    static readonly Dictionary<Shortcut, InputAction.Properties> inputExplicitProperties = new Dictionary<Shortcut, InputAction.Properties>()
    {
        { Shortcut.ActionHistoryRedo,       new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.ActionHistoryUndo,       new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.ChordSelect,             new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.ClipboardCopy,           new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.ClipboardCut,            new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.ClipboardPaste,          new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.Delete,                  new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.FileLoad,                new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.FileNew,                 new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.FileSave,                new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.FileSaveAs,              new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.PlayPause,               new InputAction.Properties {rebindable = false, hiddenInLists = false, category = Category.Global } },
        { Shortcut.SectionJumpMouseScroll,  new InputAction.Properties {rebindable = false, hiddenInLists = true, category = Category.Global } },
    };

    public static List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    [System.Serializable]
    public class ShortcutActionContainer : IEnumerable<InputAction>
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
                Shortcut scEnum = (Shortcut)i;
                InputAction.Properties properties;
                if (!inputExplicitProperties.TryGetValue(scEnum, out properties))
                {
                    properties = kDefaultProperties;
                }

                actionConfigCleanLookup[i] = new InputAction(scEnum.ToString(), properties);
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
                    actionConfigCleanLookup[(int)enumVal].kbMaps = keyVal.input.kbMaps;
                    // Add more maps as needed
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

                InputAction.Properties properties;
                if (!inputExplicitProperties.TryGetValue(sc, out properties))
                {
                    properties = kDefaultProperties;
                }

                if (!properties.rebindable)
                    continue;

                var newItem = new InputSaveData();
                newItem.action = sc.ToString();
                newItem.input = actionConfigCleanLookup[i];

                saveData.Add(newItem);

            }
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            foreach (var val in actionConfigCleanLookup)
            {
                yield return val;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return primaryInputs.GetActionConfig(key).GetInputDown(devices);
        }

        return false;
    }

    public static bool GetInputUp(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(devices);
        }

        return false;
    }

    public static bool GetInput(Shortcut key)
    {
        if (ChartEditor.hasFocus)
        {
            return primaryInputs.GetActionConfig(key).GetInput(devices);
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
