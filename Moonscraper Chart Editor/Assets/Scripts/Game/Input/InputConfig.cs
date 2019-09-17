using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConfig : ScriptableObject
{
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
}

[System.Serializable]
public class ShortcutInputConfig
{
    public Shortcut shortcut;
    public InputConfig.Properties properties;
}