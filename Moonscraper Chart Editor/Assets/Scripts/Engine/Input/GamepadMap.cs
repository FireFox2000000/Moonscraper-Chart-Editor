using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
    {
        [Serializable]
        public class GamepadButtonMap : IInputMap, IEnumerable<GamepadDevice.Button>
        {
            static System.Text.StringBuilder sb = new System.Text.StringBuilder();

            public List<GamepadDevice.Button> buttons = new List<GamepadDevice.Button>();

            public bool IsEmpty
            {
                get
                {
                    return buttons.Count <= 0;
                }
            }

            public void Add(GamepadDevice.Button key)
            {
                buttons.Add(key);
            }

            public IInputMap Clone()
            {
                GamepadButtonMap clone = new GamepadButtonMap();

                clone.buttons.AddRange(buttons);

                return clone;
            }

            public string GetInputStr()
            {
                sb.Clear();

                foreach (GamepadDevice.Button key in buttons)
                {
                    if (sb.Length > 0)
                        sb.Append(" + ");

                    sb.Append(key);
                }

                return sb.ToString();
            }

            public bool HasConflict(IInputMap other)
            {
                GamepadButtonMap otherGpMap = other as GamepadButtonMap;
                if (otherGpMap == null || otherGpMap.IsEmpty)
                    return false;

                foreach (var button in buttons)
                {
                    foreach (var otherButton in otherGpMap.buttons)
                    {
                        if (button == otherButton)
                            return true;
                    }
                }

                return false;
            }

            public void SetEmpty()
            {
                buttons.Clear();
            }

            public bool SetFrom(IInputMap that)
            {
                GamepadButtonMap gpCast = that as GamepadButtonMap;
                if (gpCast == null)
                {
                    Debug.LogError("Type incompatibility when trying to call SetFrom on a keyboard input map");
                    return false;
                }

                buttons.Clear();
                buttons.AddRange(gpCast.buttons);

                return true;
            }

            public DeviceType CompatibleDevice => DeviceType.Gamepad;

            public IEnumerator<GamepadDevice.Button> GetEnumerator()
            {
                return ((IEnumerable<GamepadDevice.Button>)buttons).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return buttons.GetEnumerator();
            }
        }
    }
}