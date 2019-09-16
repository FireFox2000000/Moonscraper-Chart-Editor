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

    ToolNoteLane1,
    ToolNoteLane2,
    ToolNoteLane3,
    ToolNoteLane4,
    ToolNoteLane5,
    ToolNoteLane6,
    ToolNoteLaneOpen,

    CloseMenu,
}

public static class ShortcutInput
{
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public const int Global = 0;
        public const int KeyboardMode = 1;
        public const int ToolNote = 2;

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(3);

        static Category()
        {
            interactionMatrix.SetInteractableAll(Global);

            interactionMatrix.SetInteractable(KeyboardMode, KeyboardMode);

            interactionMatrix.SetInteractable(ToolNote, ToolNote);
        }
    }

    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const int kCategoryDefault = Category.Global;

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    static readonly Dictionary<Shortcut, InputAction.Properties> inputExplicitProperties = new Dictionary<Shortcut, InputAction.Properties>()
    {
        { Shortcut.ActionHistoryRedo,       new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ActionHistoryUndo,       new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ChordSelect,             new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardCopy,           new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardCut,            new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardPaste,          new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.Delete,                  new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileLoad,                new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileNew,                 new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileSave,                new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileSaveAs,              new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.PlayPause,               new InputAction.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.SectionJumpMouseScroll,  new InputAction.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },

        { Shortcut.AddSongObject,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.KeyboardMode } },

        { Shortcut.ToolNoteLane1,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLane2,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLane3,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLane4,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLane5,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLane6,       new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },
        { Shortcut.ToolNoteLaneOpen,    new InputAction.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = Category.ToolNote } },

        { Shortcut.CloseMenu,           new InputAction.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },
    };

    public static List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    [System.Serializable]
    public class ShortcutActionContainer : IEnumerable<InputAction>
    {
        [SerializeField]
        List<InputAction.SaveData> saveData = new List<InputAction.SaveData>();    // Safer save data format, to handle cases where the Shortcut enum list may get updated or values are shifted around
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

        public void LoadFromSaveData(ShortcutActionContainer that)
        {
            saveData = that.saveData;
            foreach (var data in saveData)
            {
                Shortcut enumVal;
                if (System.Enum.TryParse(data.action, out enumVal))
                {
                    actionConfigCleanLookup[(int)enumVal].LoadFrom(data);
                    // Add more maps as needed
                }
                else
                {
                    Debug.LogError("Unable to parse " + data.action + " as an input action");
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

                var newItem = new InputAction.SaveData();
                actionConfigCleanLookup[i].SaveTo(sc.ToString(), newItem);

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
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown(devices);
        }

        return false;
    }

    public static bool GetInputUp(Shortcut key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(devices);
        }

        return false;
    }

    public static bool GetInput(Shortcut key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
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
