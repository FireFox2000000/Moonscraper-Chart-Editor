// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;

public class InputConfig
{
    // Default values
    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const bool kAnyDirectionAxisDefault = false;
    const MSChartEditorInput.Category.CategoryType kCategoryDefault = MSChartEditorInput.Category.CategoryType.Editor;
    const bool kAllowSameFrameMultiInputDefault = false;
    public static readonly InputConfig.Properties kDefaultProperties = new InputConfig.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault, anyDirectionAxis = kAnyDirectionAxisDefault, allowSameFrameMultiInput = kAllowSameFrameMultiInputDefault };

    [System.Serializable]
    public struct Properties
    {
        public string displayName;
        public bool rebindable;
        public bool hiddenInLists;
        public bool anyDirectionAxis;
        public MSChartEditorInput.Category.CategoryType category;
        public bool allowSameFrameMultiInput;

        public MoonscraperEngine.Input.InputAction.Properties ToMSEInputProperties()
        {
            return new MoonscraperEngine.Input.InputAction.Properties() {
                displayName = this.displayName,
                rebindable = this.rebindable,
                hiddenInLists = this.hiddenInLists,
                category = (int)this.category,
                anyDirectionAxis = this.anyDirectionAxis,
                allowSameFrameMultiInput = this.allowSameFrameMultiInput,
            };
        }
    }

    public ShortcutInputConfig[] shortcutInputs;

    public bool TryGetPropertiesConfig(MSChartEditorInputActions shortcut, out MoonscraperEngine.Input.InputAction.Properties properties)
    {
        foreach (ShortcutInputConfig config in shortcutInputs)
        {
            if (config.shortcut == shortcut)
            {
                properties = config.properties.ToMSEInputProperties();
                return true;
            }
        }

        properties = kDefaultProperties.ToMSEInputProperties();
        return false;
    }

    #region IO

    [System.Serializable]
    struct SaveInputAction
    {
        public string mSChartEditorInputAction;
        public InputConfig.Properties properties;
    }

    [System.Serializable]
    class SaveableInputConfig
    {
        public List<SaveInputAction> actionProperties = new List<SaveInputAction>();
    }

    public static void Save(InputConfig inputConfig, string path)
    {
        SaveableInputConfig saveableInputConfig = new SaveableInputConfig();
        List<SaveInputAction> actionProperties = saveableInputConfig.actionProperties;
        foreach (ShortcutInputConfig input in inputConfig.shortcutInputs)
        {
            SaveInputAction inputAction = new SaveInputAction() { mSChartEditorInputAction = input.shortcut.ToString(), properties = input.properties };
            actionProperties.Add(inputAction);
        }

        string fileText = JsonUtility.ToJson(saveableInputConfig, true);
        System.IO.File.WriteAllText(path, fileText, System.Text.Encoding.UTF8);
    }

    public static void LoadFromTextAsset(TextAsset textAsset, InputConfig inputConfig)
    {
        LoadFromJson(textAsset.text, inputConfig);
    }

    public static void LoadFromFile(string path, InputConfig inputConfig)
    {
        string jsonContents = System.IO.File.ReadAllText(path);

        LoadFromJson(jsonContents, inputConfig);
    }

    public static void LoadFromJson(string jsonFileContents, InputConfig inputConfig)
    {
        var saveableInputConfig = JsonUtility.FromJson<SaveableInputConfig>(jsonFileContents);  

        inputConfig.shortcutInputs = new ShortcutInputConfig[EnumX<MSChartEditorInputActions>.Count];

        foreach (var inputProperty in saveableInputConfig.actionProperties)
        {
            MSChartEditorInputActions action;
            if (System.Enum.TryParse(inputProperty.mSChartEditorInputAction, out action))
            {
                ShortcutInputConfig scInputConfig = new ShortcutInputConfig();
                scInputConfig.shortcut = action;
                scInputConfig.properties = inputProperty.properties;
                inputConfig.shortcutInputs[(int)action] = scInputConfig;
            }
        }

        foreach (MSChartEditorInputActions msAction in EnumX<MSChartEditorInputActions>.Values) 
        {
            if (inputConfig.shortcutInputs[(int)msAction] == null)
            {
                // Populate a default onto it
                ShortcutInputConfig scInputConfig = new ShortcutInputConfig();
                scInputConfig.shortcut = msAction;

                inputConfig.shortcutInputs[(int)msAction] = scInputConfig;
            }

            Debug.Assert(inputConfig.shortcutInputs[(int)msAction].shortcut == msAction);
        }
    }

    #endregion
}

[System.Serializable]
public class ShortcutInputConfig
{
    public MSChartEditorInputActions shortcut;
    public InputConfig.Properties properties;
}
