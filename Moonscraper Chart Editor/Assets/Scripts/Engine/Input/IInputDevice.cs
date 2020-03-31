﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
    {
        public enum DeviceType
        {
            Keyboard,
            Gamepad,
            Joystick,
        }

        public interface IInputDevice
        {
            bool Connected { get; }
            DeviceType Type { get; }
            IInputMap GetCurrentInput(InputAction.Properties properties);
            IInputMap MakeDefaultMap();
            string GetDeviceName();

            bool GetInputDown(IInputMap map);
            bool GetInputUp(IInputMap map);
            bool GetInput(IInputMap map);
            float? GetAxis(IInputMap map);
        }
    }
}
