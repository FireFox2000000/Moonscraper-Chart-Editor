using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
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

            public bool HasConflict(IInputMap other)
            {
                GamepadMap otherGpMap = other as GamepadMap;
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
                    Debug.LogError("Type incompatibility when trying to call SetFrom on a keyboard input map");
                    return false;
                }

                SetEmpty();
                buttons.AddRange(gpCast.buttons);
                axes.AddRange(gpCast.axes);

                return true;
            }

            public DeviceType CompatibleDevice => DeviceType.Gamepad;

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
        }
    }
}