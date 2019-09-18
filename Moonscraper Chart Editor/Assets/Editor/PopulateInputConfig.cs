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
          
        if (GUILayout.Button("Build Input From Scratch"))
        {
            Repopulate(inputConfigDatabase);
        }

        if (GUILayout.Button("Build Input Keep Names"))
        {
            Repopulate(inputConfigDatabase, true);
        }
    }

    void Repopulate(InputConfig inputConfigDatabase, bool preserveDisplayNames = false)
    {
        ShortcutInput.ShortcutActionContainer controls = new ShortcutInput.ShortcutActionContainer();

        ShortcutInputConfig[] shortcutInputs = new ShortcutInputConfig[Enum.GetValues(typeof(Shortcut)).Length];

        for (int i = 0; i < shortcutInputs.Length; ++i)
        {
            Shortcut scEnum = (Shortcut)i;

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

            if (preserveDisplayNames)
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
    static readonly ShortcutInput.Category.CategoryType kCategoryDefault = kDefaultProperties.category;
    

    static readonly Dictionary<Shortcut, InputConfig.Properties> inputExplicitProperties = new Dictionary<Shortcut, InputConfig.Properties>()
    {
        { Shortcut.ActionHistoryRedo,       new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ActionHistoryUndo,       new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ChordSelect,             new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardCopy,           new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardCut,            new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.ClipboardPaste,          new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.Delete,                  new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileLoad,                new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileNew,                 new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileSave,                new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.FileSaveAs,              new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.PlayPause,               new InputConfig.Properties {rebindable = false, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault } },
        { Shortcut.SectionJumpMouseScroll,  new InputConfig.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },

        { Shortcut.AddSongObject,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.KeyboardMode } },

        { Shortcut.ToolNoteLane1,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLane2,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLane3,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLane4,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLane5,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLane6,       new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },
        { Shortcut.ToolNoteLaneOpen,    new InputConfig.Properties {rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = ShortcutInput.Category.CategoryType.ToolNote } },

        { Shortcut.CloseMenu,           new InputConfig.Properties {rebindable = false, hiddenInLists = true, category = kCategoryDefault } },
    };
}
