using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConfig : ScriptableObject
{
    // Default values
    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const ShortcutInput.Category.CategoryType kCategoryDefault = ShortcutInput.Category.CategoryType.Editor;
    public static readonly InputConfig.Properties kDefaultProperties = new InputConfig.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    [System.Serializable]
    public struct Properties
    {
        public string displayName;
        public bool rebindable;
        public bool hiddenInLists;
        public ShortcutInput.Category.CategoryType category;

        public MSE.Input.InputAction.Properties ToMSEInputProperties()
        {
            return new MSE.Input.InputAction.Properties() {
                displayName = this.displayName,
                rebindable = this.rebindable,
                hiddenInLists = this.hiddenInLists,
                category = (int)this.category,
            };
        }
    }

    public ShortcutInputConfig[] shortcutInputs;

    public bool TryGetPropertiesConfig(Shortcut shortcut, out MSE.Input.InputAction.Properties properties)
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
}

[System.Serializable]
public class ShortcutInputConfig
{
    public Shortcut shortcut;
    public InputConfig.Properties properties;
}
