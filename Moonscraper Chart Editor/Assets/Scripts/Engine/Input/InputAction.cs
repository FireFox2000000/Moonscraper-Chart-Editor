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
            public class Maps : IEnumerable<IInputMap>
            {
                public List<KeyboardMap> kbMaps = new List<KeyboardMap>();
                public List<GamepadButtonMap> gpButtonMaps = new List<GamepadButtonMap>();

                public IEnumerator<IInputMap> GetEnumerator()
                {
                    foreach (var map in kbMaps)
                    {
                        yield return map;
                    }

                    foreach (var map in gpButtonMaps)
                    {
                        yield return map;
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public void Add(IInputMap map)
                {
                    KeyboardMap kbMap = map as KeyboardMap;
                    if (kbMap != null)
                        kbMaps.Add(kbMap);

                    GamepadButtonMap gpButtonMap = map as GamepadButtonMap;
                    if (gpButtonMap != null)
                        gpButtonMaps.Add(gpButtonMap);
                }
            }

            [System.Serializable]
            public class SaveData
            {
                public string action;
                public Maps input;
            }

            public Properties properties;
            Maps inputMaps = new Maps();

            public InputAction(Properties properties)
            {
                this.properties = properties;
            }

            public void LoadFrom(SaveData saveData)
            {
                inputMaps = saveData.input;
            }

            public void Add(IInputMap map)
            {
                inputMaps.Add(map);
            }

            public void SaveTo(string actionName, SaveData saveData)
            {
                saveData.action = actionName;
                saveData.input = inputMaps;
            }

            public bool HasConflict(IInputMap map)
            {
                foreach(var inputMap in inputMaps)
                {
                    if(inputMap.HasConflict(map))
                    {
                        return true;
                    }
                }

                return false;
            }

            public List<IInputMap> GetMapsForDevice(DeviceType device)
            {
                List<IInputMap> deviceMaps = new List<IInputMap>();

                foreach (IInputMap map in inputMaps)
                {
                    if (map.CompatibleDevice == device)
                        deviceMaps.Add(map);
                }

                return deviceMaps;
            }

            public void RemoveMapsForDevice(DeviceType device)
            {
                for (int i = inputMaps.kbMaps.Count - 1; i >= 0; --i)
                {
                    if (inputMaps.kbMaps[i].CompatibleDevice == device)
                        inputMaps.kbMaps.RemoveAt(i);
                }

                for (int i = inputMaps.gpButtonMaps.Count - 1; i >= 0; --i)
                {
                    if (inputMaps.gpButtonMaps[i].CompatibleDevice == device)
                        inputMaps.gpButtonMaps.RemoveAt(i);
                }
            }

            public bool GetInputDown(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];

                    foreach (var map in inputMaps)
                    {
                        if (map != null && !map.IsEmpty)
                        {
                            if (device.GetInputDown(map))
                                return true;
                        }
                    }
                }

                return false;
            }

            public bool GetInputUp(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];

                    foreach (var map in inputMaps)
                    {
                        if (map != null && !map.IsEmpty)
                        {
                            if (device.GetInputUp(map))
                                return true;
                        }
                    }
                }

                return false;
            }

            public bool GetInput(IList<IInputDevice> devices)
            {
                for (int i = 0; i < devices.Count; ++i)
                {
                    IInputDevice device = devices[i];

                    foreach (var map in inputMaps)
                    {
                        if (map != null && !map.IsEmpty)
                        {
                            if (device.GetInput(map))
                                return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
