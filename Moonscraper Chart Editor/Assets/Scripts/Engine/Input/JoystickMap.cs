// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine.Input
{
    [Serializable]
    public class JoystickMap : IInputMap, IEnumerable
    {
        static System.Text.StringBuilder sb = new System.Text.StringBuilder();

        [Serializable]
        public struct ButtonConfig
        {
            public int buttonIndex;
        }

        [Serializable]
        public struct AxisConfig
        {
            public int axisIndex;
            public JoystickDevice.AxisDir dir;
        }

        [Serializable]
        public struct BallConfig
        {
            public int ballIndex;
        }

        [Serializable]
        public struct HatConfig
        {
            public int hatIndex;
            public JoystickDevice.HatPosition position;
        }

        [SerializeField]
        string deviceId;

        public List<ButtonConfig> buttons = new List<ButtonConfig>();
        public List<AxisConfig> axes = new List<AxisConfig>();
        public List<BallConfig> balls = new List<BallConfig>();    // ???
        public List<HatConfig> hats = new List<HatConfig>();

        public JoystickMap(string deviceId)
        {
            this.deviceId = deviceId;
        }

        public void Add(ButtonConfig button)
        {
            buttons.Add(button);
        }

        public void Add(IList<ButtonConfig> buttonConfigs)
        {
            buttons.AddRange(buttonConfigs);
        }

        public void Add(int axis, JoystickDevice.AxisDir dir)
        {
            axes.Add(new AxisConfig() { axisIndex = axis, dir = dir });
        }

        public void Add(BallConfig ball)
        {
            balls.Add(ball);
        }

        public void Add(int hat, JoystickDevice.HatPosition position)
        {
            hats.Add(new HatConfig() { hatIndex = hat, position = position });
        }

        public void Add(IList<HatConfig> hatConfigs)
        {
            hats.AddRange(hatConfigs);
        }

        public bool IsEmpty
        {
            get
            {
                return
                    buttons.Count <= 0 && 
                    axes.Count <= 0 && 
                    balls.Count <= 0 && 
                    hats.Count <= 0;
            }
        }

        public IInputMap Clone()
        {
            JoystickMap clone = new JoystickMap(deviceId);

            clone.buttons.AddRange(buttons);
            clone.axes.AddRange(axes);
            clone.balls.AddRange(balls);
            clone.hats.AddRange(hats);

            return clone;
        }

        public string GetInputStr()
        {
            sb.Clear();

            foreach (var index in buttons)
            {
                if (sb.Length > 0)
                    sb.Append(" + ");

                sb.AppendFormat("Button {0}", index.buttonIndex);
            }

            foreach (var axis in axes)
            {
                if (sb.Length > 0)
                    sb.Append(" + ");

                sb.AppendFormat("Axis {0}", axis.axisIndex);
            }

            foreach (var ball in balls)
            {
                if (sb.Length > 0)
                    sb.Append(" + ");

                sb.AppendFormat("Ball {0}", ball);
            }

            foreach (var hat in hats)
            {
                if (sb.Length > 0)
                    sb.Append(" + ");

                sb.AppendFormat("Hat {0} {1}", hat.hatIndex, hat.position.ToString());
            }

            return sb.ToString();
        }

        public bool HasConflict(IInputMap other, InputAction.Properties properties)
        {
            JoystickMap otherMap = other as JoystickMap;
            if (otherMap == null || otherMap.IsEmpty)
                return false;

            bool allowSameFrameMultiInput = properties.allowSameFrameMultiInput;

            if (allowSameFrameMultiInput)
            {
                if (buttons.Count > 0 && otherMap.buttons.Count > 0)
                {
                    // Check if they match exactly, or if one map is a sub-set of the other
                    var smallerButtonMap = buttons.Count < otherMap.buttons.Count ? buttons : otherMap.buttons;
                    var largerButtonMap = buttons.Count < otherMap.buttons.Count ? otherMap.buttons : buttons;

                    int sameInputCount = 0;
                    foreach (var button in smallerButtonMap)
                    {
                        bool contains = false;
                        foreach (var otherButton in largerButtonMap)
                        {
                            if (button.buttonIndex == otherButton.buttonIndex)
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (contains)
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
                    foreach (var otherButton in otherMap.buttons)
                    {
                        if (button.buttonIndex == otherButton.buttonIndex)
                            return true;
                    }
                }
            }

            foreach (var axis in axes)
            {
                foreach (var otherAxis in otherMap.axes)
                {
                    if (axis.axisIndex == otherAxis.axisIndex && (axis.dir == otherAxis.dir || axis.dir == JoystickDevice.AxisDir.Any || otherAxis.dir == JoystickDevice.AxisDir.Any))
                        return true;
                }
            }

            foreach (var ballIndex in balls)
            {
                foreach (var otherBallIndex in otherMap.balls)
                {
                    if (ballIndex.ballIndex == otherBallIndex.ballIndex)
                        return true;
                }
            }

            foreach (var hat in hats)
            {
                foreach (var otherHat in otherMap.hats)
                {
                    if (hat.hatIndex == otherHat.hatIndex && hat.position == otherHat.position)
                        return true;
                }
            }

            return false;
        }

        public void SetEmpty()
        {
            buttons.Clear();
            axes.Clear();
            balls.Clear();
            hats.Clear();
        }

        public bool SetFrom(IInputMap that)
        {
            JoystickMap jsCast = that as JoystickMap;
            if (jsCast == null)
            {
                Debug.LogError("Type incompatibility when trying to call SetFrom on a joystick input map");
                return false;
            }

            SetEmpty();
            buttons.AddRange(jsCast.buttons);
            axes.AddRange(jsCast.axes);
            balls.AddRange(jsCast.balls);
            hats.AddRange(jsCast.hats);

            return true;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var button in buttons)
            {
                yield return button;
            }

            foreach (var axis in axes)
            {
                yield return axis;
            }

            foreach (var ball in balls)
            {
                yield return ball;
            }

            foreach (var hat in hats)
            {
                yield return hat;
            }
        }

        public bool IsCompatibleWithDevice(IInputDevice device)
        {
            if (device.Type == DeviceType.Joystick)
            {
                var jsDevice = device as JoystickDevice;
                return jsDevice.deviceId == deviceId;
            }

            return false;
        }
    }
}
