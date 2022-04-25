// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine.Input
{
    [Serializable]
    public class KeyboardMap : IInputMap, IEnumerable<KeyCode>
    {
        static System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
        public KeyboardDevice.ModifierKeys modifiers = KeyboardDevice.ModifierKeys.None;
        public List<KeyCode> keys = new List<KeyCode>();

        public KeyboardMap(KeyboardDevice.ModifierKeys modifiers = KeyboardDevice.ModifierKeys.None)
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

            if (modifiers != KeyboardDevice.ModifierKeys.None)
            {
                for (int i = 1; i < EnumX<KeyboardDevice.ModifierKeys>.Count; ++i)
                {
                    KeyboardDevice.ModifierKeys modifierEnum = EnumX<KeyboardDevice.ModifierKeys>.Values[i];

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


        public bool HasConflict(IInputMap other, InputAction.Properties properties)
        {
            KeyboardMap otherKbMap = other as KeyboardMap;
            if (otherKbMap == null || otherKbMap.IsEmpty)
                return false;

            KeyboardDevice.ModifierKeys manualModifierKeysPressed = KeyboardDevice.ModifierKeys.None;
            foreach (KeyCode otherKeyCode in otherKbMap.keys)
            {
                manualModifierKeysPressed |= KeyboardDevice.ToModifierKey(otherKeyCode);
            }

            if (keys.Count <= 0 && modifiers == manualModifierKeysPressed && modifiers != KeyboardDevice.ModifierKeys.None)
                return true;

            if (modifiers != otherKbMap.modifiers)
                return false;

            foreach (KeyCode keyCode in keys)
            {
                foreach (KeyCode otherKeyCode in otherKbMap.keys)
                {
                    bool modifierKeyConflict = (KeyboardDevice.ToModifierKey(keyCode) & otherKbMap.modifiers) != 0;

                    if (modifierKeyConflict || keyCode == otherKeyCode)
                        return true;
                }
            }  

            return false;
        }

        public bool IsEmpty
        {
            get
            {
                return modifiers == KeyboardDevice.ModifierKeys.None && keys.Count <= 0;
            }
        }

        public IEnumerator<KeyCode> GetEnumerator()
        {
            return ((IEnumerable<KeyCode>)keys).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyCode>)keys).GetEnumerator();
        }

        public IInputMap Clone()
        {
            KeyboardMap clone = new KeyboardMap();

            clone.modifiers = modifiers;
            clone.keys.AddRange(keys);

            return clone;
        }

        public bool SetFrom(IInputMap that)
        {
            KeyboardMap kbCast = that as KeyboardMap;
            if (kbCast == null)
            {
                Debug.LogError("Type incompatibility when trying to call SetFrom on a keyboard input map");
                return false;
            }

            modifiers = kbCast.modifiers;
            keys.Clear();
            keys.AddRange(kbCast.keys);

            return true;
        }

        public void SetEmpty()
        {
            modifiers = KeyboardDevice.ModifierKeys.None;
            keys.Clear();
        }

        public bool IsCompatibleWithDevice(IInputDevice device)
        {
            return device.Type == DeviceType.Keyboard;
        }
    }
}