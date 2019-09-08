using System.Collections;
using System.Collections.Generic;

namespace MSE
{
    namespace Input
    {
        [System.Serializable]
        public class InputAction
        {
            public struct Properties
            {
                public bool rebindable;
                public bool hiddenInLists;
            }

            public const int kMaxKeyboardMaps = 2;

            [System.NonSerialized]      // We set this ourselves
            public Properties properties;
            public string displayName { get; private set; }

            public KeyboardMap[] kbMaps = new KeyboardMap[kMaxKeyboardMaps];

            public InputAction(string displayName, Properties properties)
            {
                this.displayName = displayName;
                this.properties = properties;
            }

            public bool HasConflict(IInputMap map)
            {
                for (int i = 0; i < kbMaps.Length; ++i)
                {
                    KeyboardMap kbMap = kbMaps[i];
                    if(kbMap.HasConflict(map))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool GetInputDown(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];
                    if (device.Type == DeviceType.Keyboard)
                    {
                        KeyboardDevice keyboardDevice = (KeyboardDevice)device;

                        for (int mapIndex = 0; mapIndex < kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = kbMaps[mapIndex];
                            if (map != null)
                            {
                                if (keyboardDevice.GetInputDown(map))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                return false;
            }

            public bool GetInputUp(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];
                    if (device.Type == DeviceType.Keyboard)
                    {
                        KeyboardDevice keyboardDevice = (KeyboardDevice)device;

                        for (int mapIndex = 0; mapIndex < kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = kbMaps[mapIndex];
                            if (map != null)
                            {
                                if (keyboardDevice.GetInputUp(map))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                return false;
            }

            public bool GetInput(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];
                    if (device.Type == DeviceType.Keyboard)
                    {
                        KeyboardDevice keyboardDevice = (KeyboardDevice)device;

                        for (int mapIndex = 0; mapIndex < kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = kbMaps[mapIndex];
                            if (map != null)
                            {
                                if (keyboardDevice.GetInput(map))
                                    return true;
                            }
                        }

                        return false;
                    }
                }

                return false;
            }

            public IInputMap[] GetMapsForDevice(DeviceType device)
            {
                switch (device)
                {
                    case DeviceType.Keyboard: return kbMaps;
                    default: return null;
                }
            }
        }
    }
}
