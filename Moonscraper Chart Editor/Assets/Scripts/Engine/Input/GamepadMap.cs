// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine.Input
{
    [Serializable]
    public class GamepadMap : IInputMap, IEnumerable
    {
        [Serializable]
        public struct AxisConfig
        {
            public GamepadDevice.Axis axis;
            public GamepadDevice.AxisDir dir;
        }

        static System.Text.StringBuilder sb = new System.Text.StringBuilder();

        public List<GamepadDevice.Button> buttons = new List<GamepadDevice.Button>();
        public List<AxisConfig> axes = new List<AxisConfig>();

        public bool IsEmpty
        {
            get
            {
                return buttons.Count <= 0 && axes.Count <= 0;
            }
        }

        public void Add(GamepadDevice.Button key)
        {
            buttons.Add(key);
        }

        public void Add(IList<GamepadDevice.Button> keys)
        {
            buttons.AddRange(keys);
        }

        public void Add(GamepadDevice.Axis axis, GamepadDevice.AxisDir dir)
        {
            axes.Add(new AxisConfig() { axis = axis, dir = dir });
        }

        public IInputMap Clone()
        {
            GamepadMap clone = new GamepadMap();

            clone.buttons.AddRange(buttons);
            clone.axes.AddRange(axes);

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

            foreach (var axis in axes)
            {
                if (sb.Length > 0)
                    sb.Append(" + ");

                sb.Append(axis.axis.ToString());
            }

            return sb.ToString();
        }

        public bool HasConflict(IInputMap other, InputAction.Properties properties)
        {
            GamepadMap otherGpMap = other as GamepadMap;
            if (otherGpMap == null || otherGpMap.IsEmpty)
                return false;

            bool allowSameFrameMultiInput = properties.allowSameFrameMultiInput;

            if (allowSameFrameMultiInput)
            {
                if (buttons.Count > 0 && otherGpMap.buttons.Count > 0)
                {
                    // Check if they match exactly, or if one map is a sub-set of the other
                    var smallerButtonMap = buttons.Count < otherGpMap.buttons.Count ? buttons : otherGpMap.buttons;
                    var largerButtonMap = buttons.Count < otherGpMap.buttons.Count ? otherGpMap.buttons : buttons;

                    int sameInputCount = 0;
                    foreach (var button in smallerButtonMap)
                    {
                        if (largerButtonMap.Contains(button))
                        {
                            ++sameInputCount;
                        }
                    }

                    if (sameInputCount == smallerButtonMap.Count)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var button in buttons)
                {
                    foreach (var otherButton in otherGpMap.buttons)
                    {
                        if (button == otherButton)
                            return true;
                    }
                }
            }

            foreach (var axis in axes)
            {
                foreach (var otherAxis in otherGpMap.axes)
                {
                    if (axis.axis == otherAxis.axis && (axis.dir == otherAxis.dir || axis.dir == GamepadDevice.AxisDir.Any || otherAxis.dir == GamepadDevice.AxisDir.Any))
                        return true;
                }
            }

            return false;
        }

        public void SetEmpty()
        {
            buttons.Clear();
            axes.Clear();
        }

        public bool SetFrom(IInputMap that)
        {
            GamepadMap gpCast = that as GamepadMap;
            if (gpCast == null)
            {
                Debug.LogError("Type incompatibility when trying to call SetFrom on a gamepad input map");
                return false;
            }

            SetEmpty();
            buttons.AddRange(gpCast.buttons);
            axes.AddRange(gpCast.axes);

            return true;
        }

        //public IEnumerator<GamepadDevice.Button> GetEnumerator()
        //{
        //    return ((IEnumerable<GamepadDevice.Button>)buttons).GetEnumerator();
        //}
            
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var button in buttons)
            {
                yield return button;
            }

            foreach (var axis in axes)
            {
                yield return axis;
            }
        }

        public bool IsCompatibleWithDevice(IInputDevice device)
        {
            return device.Type == DeviceType.Gamepad;
        }
    }
}