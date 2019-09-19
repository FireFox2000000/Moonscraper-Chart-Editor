using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSE
{
    namespace Input
    {
        public class GamepadDevice : IInputDevice
        {
            int? m_padIndex;

            public GamepadDevice(int? padIndex = null)
            {
                m_padIndex = padIndex;
            }

            public bool Connected => throw new System.NotImplementedException();

            public DeviceType Type => DeviceType.Gamepad;

            public IInputMap GetCurrentInput()
            {
                throw new System.NotImplementedException();
            }

            public string GetDeviceName()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}