using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
    {
        public class KeyboardDevice : IInputDevice
        {
            [Flags]
            public enum ModifierKeys
            {
                None = 0,
                Ctrl = 1 << 1,
                Shift = 1 << 2,
                Alt = 1 << 3,
            }
            public static ModifierKeys[] keyEnums = (ModifierKeys[])Enum.GetValues(typeof(ModifierKeys));

            static readonly System.Array allKeyCodes = System.Enum.GetValues(typeof(KeyCode));
            delegate bool InputFn(KeyCode keyCode);
            static InputFn inputDownFn = UnityEngine.Input.GetKeyDown;
            static InputFn inputUpFn = UnityEngine.Input.GetKeyUp;
            static InputFn inputGetFn = UnityEngine.Input.GetKey;
            static bool ctrlKeyBeingPressed { get { return UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl); } }
            static bool shiftKeyBeingPressed { get { return UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift); } }
            static bool atlKeyBeingPressed { get { return UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt); } }

            public bool Connected { get { return true; } }
            public DeviceType Type { get { return DeviceType.Keyboard; } }

            static List<KeyCode> currentKeys = new List<KeyCode>();
            static List<KeyCode> releasedModifierKeys = new List<KeyCode>();
            public IInputMap GetCurrentInput()
            {
                currentKeys.Clear();
                releasedModifierKeys.Clear();

                ModifierKeys modifiersActive = ModifierKeys.None;
                bool containsNonModifierKey = false;

                foreach (KeyCode kCode in allKeyCodes)
                {
                    if ((int)kCode >= (int)KeyCode.Menu)
                        break;

                    bool isModifierKey = IsModifierKey(kCode);
                    if (UnityEngine.Input.GetKey(kCode))
                    {
                        currentKeys.Add(kCode);
                        modifiersActive |= ToModifierKey(kCode);

                        if (!isModifierKey)
                        {
                            containsNonModifierKey = true;
                        }
                    }

                    if (isModifierKey && UnityEngine.Input.GetKeyUp(kCode))
                    {
                        releasedModifierKeys.Add(kCode);
                    }
                }

                // Create map if no modifier keys are active, or if they are, they are released or have another input with it
                if (currentKeys.Count > 0 && containsNonModifierKey)
                {
                    KeyboardMap map = new KeyboardMap(modifiersActive);

                    foreach(KeyCode kCode in currentKeys)
                    {
                        if (!IsModifierKey(kCode))
                            map.Add(kCode);
                    }

                    return map;
                }

                // If we wanted to use modifier keys as regular keys
                if (currentKeys.Count <= 0 && releasedModifierKeys.Count == 1)
                {
                    KeyboardMap map = new KeyboardMap(ModifierKeys.None);

                    foreach (KeyCode kCode in releasedModifierKeys)
                    {
                        map.Add(kCode);
                    }

                    return map;
                }

                return null;
            }

            public static bool IsModifierKey(KeyCode key)
            {
                return ToModifierKey(key) != ModifierKeys.None;
            }

            public static ModifierKeys ToModifierKey(KeyCode key)
            {
                switch (key)
                {
                    case KeyCode.LeftControl:
                    case KeyCode.RightControl:
                        return ModifierKeys.Ctrl;

                    case KeyCode.LeftShift:
                    case KeyCode.RightShift:
                        return ModifierKeys.Shift;

                    case KeyCode.LeftAlt:
                    case KeyCode.RightAlt:
                        return ModifierKeys.Alt;

                    default: break;
                }

                return ModifierKeys.None;
            }

            /////////////// Input evaluation functions /////////////////////

            static bool CheckKey(KeyCode key, InputFn InputFn)
            {
                if (InputFn(key))
                    return true;

                return false;
            }

            public bool GetInputDown(KeyboardMap map)
            {
                if (!CheckDesiredModifierKeysActive(map.modifiers))
                    return false;

                if (map.modifiers == ModifierKeys.None && map.keys.Count <= 0)
                    return false;

                // Look for at least 1 key that is coming down. The rest just need to be pressed
                bool hasInputDir = false;
                foreach (KeyCode keyCode in map.keys)
                {
                    if (!CheckKey(keyCode, inputDownFn))
                    {
                        if (!CheckKey(keyCode, inputGetFn))
                            return false;
                    }
                    else
                    {
                        hasInputDir = true;
                    }
                }

                return hasInputDir;
            }

            public bool GetInputUp(KeyboardMap map)
            {
                if (!CheckDesiredModifierKeysActive(map.modifiers))
                    return false;

                if (map.modifiers == ModifierKeys.None && map.keys.Count <= 0)
                    return false;

                // Look for at least 1 key that is coming up. The rest just need to be pressed
                bool hasInputDir = false;
                foreach (KeyCode keyCode in map.keys)
                {
                    if (!CheckKey(keyCode, inputUpFn))
                    {
                        if (!CheckKey(keyCode, inputGetFn))
                            return false;
                    }
                    else
                    {
                        hasInputDir = true;
                    }
                }

                return hasInputDir;
            }

            public bool GetInput(KeyboardMap map)
            {
                if (!CheckDesiredModifierKeysActive(map.modifiers))
                    return false;

                if (map.modifiers == ModifierKeys.None && map.keys.Count <= 0)
                    return false;

                foreach (KeyCode keyCode in map.keys)
                {
                    if (!CheckKey(keyCode, inputGetFn))
                        return false;
                }

                return true;
            }

            bool CheckDesiredModifierKeysActive(ModifierKeys modifiers)
            {
                if (modifiers == ModifierKeys.None)
                {
                    return !ctrlKeyBeingPressed && !shiftKeyBeingPressed && !atlKeyBeingPressed;
                }

                ModifierKeys currentModiKeys = ModifierKeys.None;

                for (int i = 1; i < keyEnums.Length; ++i)
                {
                    ModifierKeys modifierEnum = keyEnums[i];

                    bool modifierInputActive = false;

                    switch (modifierEnum)
                    {
                        case ModifierKeys.Ctrl:
                            modifierInputActive = ctrlKeyBeingPressed;
                            break;

                        case ModifierKeys.Shift:
                            modifierInputActive = shiftKeyBeingPressed;
                            break;

                        case ModifierKeys.Alt:
                            modifierInputActive = atlKeyBeingPressed;
                            break;

                        default:
                            Debug.LogError("No inputs defined for modifier " + modifierEnum);
                            break;
                    }

                    if (modifierInputActive)
                        currentModiKeys |= modifierEnum;
                }

                return currentModiKeys == modifiers;
            }
        }
    }
}