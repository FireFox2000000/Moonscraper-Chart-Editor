using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
    {
        [Serializable]
        public class KeyboardMap : IInputMap, IEnumerable<KeyCode>
        {
            [Flags]
            public enum ModifierKeys
            {
                None    = 0,
                Ctrl    = 1 << 1,
                Shift   = 1 << 2,
                Alt     = 1 << 3,
            }

            static ModifierKeys[] keyEnums = (ModifierKeys[])Enum.GetValues(typeof(ModifierKeys));

            delegate bool InputFn(KeyCode keyCode);
            static InputFn inputDownFn = UnityEngine.Input.GetKeyDown;
            static InputFn inputUpFn = UnityEngine.Input.GetKeyUp;
            static InputFn inputGetFn = UnityEngine.Input.GetKey;
            static System.Text.StringBuilder sb = new System.Text.StringBuilder();

            [SerializeField]
            ModifierKeys modifiers = ModifierKeys.None;
            [SerializeField]
            List<KeyCode> keys = new List<KeyCode>();

            public KeyboardMap(ModifierKeys modifiers = ModifierKeys.None)
            {
                this.modifiers = modifiers;
            }

            public void Add(KeyCode key)
            {
                keys.Add(key);
            }

            public string GetInputStr()
            {
                sb.Clear();

                if (modifiers != ModifierKeys.None)
                {
                    for (int i = 1; i < keyEnums.Length; ++i)
                    {
                        ModifierKeys modifierEnum = keyEnums[i];

                        if ((modifiers & modifierEnum) != 0)
                        {
                            if (sb.Length > 0)
                                sb.Append(" + ");

                            sb.Append(modifierEnum);
                        }
                    }
                }

                foreach (KeyCode key in keys)
                {
                    if (sb.Length > 0)
                        sb.Append(" + ");

                    sb.Append(key);
                }

                return sb.ToString();
            }


            public bool HasConflict(IInputMap other)
            {
                KeyboardMap otherKbMap = other as KeyboardMap;
                if (otherKbMap == null)
                    return false;

                if (modifiers != otherKbMap.modifiers && modifiers != ModifierKeys.None && otherKbMap.modifiers != ModifierKeys.None)
                    return false;

                foreach (KeyCode keyCode in keys)
                {
                    foreach (KeyCode otherKeyCode in otherKbMap.keys)
                    {
                        if (keyCode == otherKeyCode)
                            return true;
                    }
                }

                return false;
            }

            public IEnumerator<KeyCode> GetEnumerator()
            {
                return ((IEnumerable<KeyCode>)keys).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyCode>)keys).GetEnumerator();
            }

            /////////////// Input evaluation functions /////////////////////

            static bool CheckKey(KeyCode key, InputFn InputFn)
            {
                if (InputFn(key))
                    return true;

                return false;
            }

            static bool CheckModifierInputIsActive(ModifierKeys modifiers)
            {
                if (modifiers == ModifierKeys.None)
                    return true;

                for (int i = 1; i < keyEnums.Length; ++i)
                {
                    ModifierKeys modifierEnum = keyEnums[i];

                    if ((modifiers & modifierEnum) != 0)
                    {
                        bool modifierInputActive = false;

                        switch (modifierEnum)
                        {
                            case ModifierKeys.Ctrl:
                                modifierInputActive = UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
                                break;

                            case ModifierKeys.Shift:
                                modifierInputActive = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
                                break;

                            case ModifierKeys.Alt:
                                modifierInputActive = UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt);
                                break;

                            default:
                                Debug.LogError("No inputs defined for modifier " + modifierEnum);
                                break;
                        }

                        if (!modifierInputActive)
                            return false;
                    }
                }

                return true;
            }

            public static bool GetInputDown(KeyboardMap map)
            {
                if (!CheckModifierInputIsActive(map.modifiers) || map.keys.Count <= 0)
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

            public static bool GetInputUp(KeyboardMap map)
            {
                if (!CheckModifierInputIsActive(map.modifiers) || map.keys.Count <= 0)
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

            public static bool GetInput(KeyboardMap map)
            {
                if (!CheckModifierInputIsActive(map.modifiers) || map.keys.Count <= 0)
                    return false;

                foreach (KeyCode keyCode in map.keys)
                {
                    if (!CheckKey(keyCode, inputGetFn))
                        return false;
                }

                return true;
            }
        }
    }
}