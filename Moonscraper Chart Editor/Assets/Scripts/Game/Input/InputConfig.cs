using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConfig : ScriptableObject
{
    // Default values
    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const ShortcutInput.Category.CategoryType kCategoryDefault = ShortcutInput.Category.CategoryType.Global;
    public static readonly InputConfig.Properties kDefaultProperties = new InputConfig.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    const GameplayInput.Category.CategoryType kGameplayCategoryDefault = GameplayInput.Category.CategoryType.Guitar;
    public static readonly InputConfig.GameplayProperties kGameplayDefaultProperties = new InputConfig.GameplayProperties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kGameplayCategoryDefault };

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

    [System.Serializable]
    public struct GameplayProperties
    {
        public string displayName;
        public bool rebindable;
        public bool hiddenInLists;
        public GameplayInput.Category.CategoryType category;

        public MSE.Input.InputAction.Properties ToMSEInputProperties()
        {
            return new MSE.Input.InputAction.Properties()
            {
                displayName = this.displayName,
                rebindable = this.rebindable,
                hiddenInLists = this.hiddenInLists,
                category = (int)this.category,
            };
        }
    }

    public ShortcutInputConfig[] shortcutInputs;
    public GameplayActionConfig[] gameplayInputs;

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

    public bool TryGetPropertiesConfig(GameplayAction action, out MSE.Input.InputAction.Properties properties)
    {
        foreach (GameplayActionConfig config in gameplayInputs)
        {
            if (config.action == action)
            {
                properties = config.properties.ToMSEInputProperties();
                return true;
            }
        }

        properties = kGameplayDefaultProperties.ToMSEInputProperties();
        return false;
    }
}

[System.Serializable]
public class ShortcutInputConfig
{
    public Shortcut shortcut;
    public InputConfig.Properties properties;
}

[System.Serializable]
public class GameplayActionConfig
{
    public GameplayAction action;
    public InputConfig.GameplayProperties properties;
}