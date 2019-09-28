using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE;
using MSE.Input;

public enum MSChartEditorInputActions
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

    // Guitar Actions
    GuitarStrumUp,
    GuitarStrumDown,

    GuitarFretGreen,
    GuitarFretRed,
    GuitarFretYellow,
    GuitarFretBlue,
    GuitarFretOrange,

    // Drum Actions
    DrumPadRed,
    DrumPadYellow,
    DrumPadBlue,
    DrumPadOrange,
    DrumPadGreen,
    DrumPadKick,
}

public static class MSChartEditorInput
{
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public enum CategoryType
        {
            Editor,
            EditorKeyboardMode,
            EditorToolNote,

            GameplayGuitar,
            GameplayDrums,
        }

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(EnumX<CategoryType>.Count);
        public static readonly int kEditorCategoryMask 
            = (1 << (int)CategoryType.Editor)
            | (1 << (int)CategoryType.EditorKeyboardMode)
            | (1 << (int)CategoryType.EditorToolNote)
            ;
        public static readonly int kGameplayCategoryMask = (1 << (int)CategoryType.GameplayGuitar) | (1 << (int)CategoryType.GameplayDrums);

        static Category()
        {
            interactionMatrix.SetInteractableAll((int)CategoryType.Editor);

            interactionMatrix.SetInteractable((int)CategoryType.EditorKeyboardMode, (int)CategoryType.EditorKeyboardMode);
            interactionMatrix.SetInteractable((int)CategoryType.EditorToolNote, (int)CategoryType.EditorToolNote);

            interactionMatrix.SetInteractable((int)CategoryType.GameplayGuitar, (int)CategoryType.GameplayGuitar);
            interactionMatrix.SetInteractable((int)CategoryType.GameplayDrums, (int)CategoryType.GameplayDrums);
        }
    }

    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const int kCategoryDefault = (int)Category.CategoryType.Editor;

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    public class MSChartEditorActionContainer : InputActionContainer<MSChartEditorInputActions>
    {
        public MSChartEditorActionContainer()  : base(new EnumLookupTable<MSChartEditorInputActions, InputAction>())
        {
            InputManager inputManager = InputManager.Instance;

            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                MSChartEditorInputActions scEnum = (MSChartEditorInputActions)i;
                InputAction.Properties properties;
                if (!inputManager.inputPropertiesConfig.TryGetPropertiesConfig(scEnum, out properties))
                {
                    properties = kDefaultProperties;
                }

                if (string.IsNullOrEmpty(properties.displayName))
                {
                    properties.displayName = scEnum.ToString();
                }

                actionConfigCleanLookup[scEnum] = new InputAction(properties);
            }
        }
    }

    static MSChartEditorActionContainer primaryInputs { get { return GameSettings.controls; } } 

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static bool GetInputDown(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInputUp(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInput(MSChartEditorInputActions key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInput(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetGroupInputDown(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInputDown(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInputUp(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(MSChartEditorInputActions[] keys)
    {
        foreach (MSChartEditorInputActions key in keys)
        {
            if (GetInput(key))
                return true;
        }

        return false;
    }
}
