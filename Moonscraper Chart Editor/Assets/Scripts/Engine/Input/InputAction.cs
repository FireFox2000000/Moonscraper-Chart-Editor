using System.Collections;
using System.Collections.Generic;

namespace MSE
{
    namespace Input
    {
        public class InputAction
        {
            public struct Properties
            {
                public string displayName;
                public bool rebindable;
                public bool hiddenInLists;
                public int category;
            }

            public const int kMaxKeyboardMaps = 2;

            [System.Serializable]
            public class Maps
            {
                public KeyboardMap[] kbMaps = new KeyboardMap[kMaxKeyboardMaps];

                public Maps()
                {
                    for (int i = 0; i < kbMaps.Length; ++i)
                    {
                        kbMaps[i] = new KeyboardMap();
                    }
                }
            }

            [System.Serializable]
            public class SaveData
            {
                public string action;
                public Maps input;
            }

            public Properties properties;
            public Maps inputMaps = new Maps();

            public InputAction(Properties properties)
            {
                this.properties = properties;
            }

            public void LoadFrom(SaveData saveData)
            {
                inputMaps = saveData.input;
            }

            public void SaveTo(string actionName, SaveData saveData)
            {
                saveData.action = actionName;
                saveData.input = inputMaps;
            }

            public bool HasConflict(IInputMap map)
            {
                for (int i = 0; i < inputMaps.kbMaps.Length; ++i)
                {
                    KeyboardMap kbMap = inputMaps.kbMaps[i];
                    if(kbMap.HasConflict(map))
                    {
                        return true;
                    }
                }

                return false;
            }

            public IInputMap[] GetMapsForDevice(DeviceType device)
            {
                switch (device)
                {
                    case DeviceType.Keyboard: return inputMaps.kbMaps;
                    default: return null;
                }
            }

            public bool GetInputDown(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];
                    if (device.Type == DeviceType.Keyboard)
                    {
                        KeyboardDevice keyboardDevice = (KeyboardDevice)device;

                        for (int mapIndex = 0; mapIndex < inputMaps.kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = inputMaps.kbMaps[mapIndex];
                            if (map != null && !map.IsEmpty)
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

                        for (int mapIndex = 0; mapIndex < inputMaps.kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = inputMaps.kbMaps[mapIndex];
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

                        for (int mapIndex = 0; mapIndex < inputMaps.kbMaps.Length; ++mapIndex)
                        {
                            KeyboardMap map = inputMaps.kbMaps[mapIndex];
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
        }
    }
}
