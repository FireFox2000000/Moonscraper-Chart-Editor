using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MSE.Input;

[CustomEditor(typeof(InputConfigBuilder))]
public class PopulateInputConfig : Editor
{
    SerializedProperty _inputConfigDatabase;
    void OnEnable()
    {
        _inputConfigDatabase = serializedObject.FindProperty("inputConfigDatabase");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_inputConfigDatabase);
        serializedObject.ApplyModifiedProperties();

        InputConfig inputConfigDatabase = ((InputConfigBuilder)target).inputConfigDatabase;
          
        if (GUILayout.Button("Build Shortcut Input From Scratch"))
        {
            RepopulateInput(inputConfigDatabase);
        }

        if (GUILayout.Button("Build Shortcut Input Keep Names"))
        {
            RepopulateInput(inputConfigDatabase, true);
        }
    }

    void RepopulateInput(InputConfig inputConfigDatabase, bool preserveDisplayNames = false)
    {
        MSChartEditorInput.MSChartEditorActionContainer controls = new MSChartEditorInput.MSChartEditorActionContainer();

        ShortcutInputConfig[] shortcutInputs = new ShortcutInputConfig[EnumX<MSChartEditorInputActions>.Count];

        for (int i = 0; i < shortcutInputs.Length; ++i)
        {
            MSChartEditorInputActions scEnum = (MSChartEditorInputActions)i;

            InputConfig.Properties properties;
            if (!inputExplicitProperties.TryGetValue(scEnum, out properties))
            {
                properties = kDefaultProperties;
            }

            if (string.IsNullOrEmpty(properties.displayName))
            {
                properties.displayName = scEnum.ToString();
            }

            ShortcutInputConfig config = new ShortcutInputConfig();
            var defaultConfig = controls.GetActionConfig(scEnum);
            var defaultProperties = defaultConfig.properties;

            config.shortcut = scEnum;
            config.properties = properties;

            if (preserveDisplayNames && i < inputConfigDatabase.shortcutInputs.Length)
            {
                config.properties.displayName = inputConfigDatabase.shortcutInputs[i].properties.displayName;
            }

            shortcutInputs[i] = config;
        }

        inputConfigDatabase.shortcutInputs = shortcutInputs;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static readonly InputConfig.Properties kDefaultProperties = InputConfig.kDefaultProperties;
    static readonly bool kRebindableDefault = kDefaultProperties.rebindable;
    static readonly bool kHiddenInListsDefault = kDefaultProperties.hiddenInLists;
    static readonly MSChartEditorInput.Category.CategoryType kCategoryDefault = kDefaultProperties.category;    

    static readonly Dictionary<MSChartEditorInputActions, InputConfig.Properties> inputExplicitProperties = new Dictionary<MSChartEditorInputActions, InputConfig.Properties>()
    {
        { MSChartEditorInputActions.ActionHistoryRedo,       new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.ActionHistoryUndo,       new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.ChordSelect,             new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.ClipboardCopy,           new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.ClipboardCut,            new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.ClipboardPaste,          new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.Delete,                  new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.FileLoad,                new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.FileNew,                 new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.FileSave,                new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.FileSaveAs,              new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.PlayPause,               new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { MSChartEditorInputActions.SectionJumpMouseScroll,  new InputConfig.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },

        { MSChartEditorInputActions.AddSongObject,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorKeyboardMode } },

        { MSChartEditorInputActions.ToolNoteLane1,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLane2,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLane3,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLane4,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLane5,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLane6,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },
        { MSChartEditorInputActions.ToolNoteLaneOpen,    new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.EditorToolNote } },

        { MSChartEditorInputActions.CloseMenu,           new InputConfig.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        { MSChartEditorInputActions.GuitarStrumUp, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarStrumDown, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarFretGreen, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarFretRed, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarFretYellow, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarFretBlue, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },
        { MSChartEditorInputActions.GuitarFretOrange, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayGuitar } },

        { MSChartEditorInputActions.DrumPadRed, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
        { MSChartEditorInputActions.DrumPadYellow, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
        { MSChartEditorInputActions.DrumPadBlue, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
        { MSChartEditorInputActions.DrumPadOrange, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
        { MSChartEditorInputActions.DrumPadGreen, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
        { MSChartEditorInputActions.DrumPadKick, new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = MSChartEditorInput.Category.CategoryType.GameplayDrums } },
    };
}
