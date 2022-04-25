// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace MoonscraperEngine.Input
{
    public class InputAction
    {
        public struct Properties
        {
            public string displayName;
            public bool rebindable;
            public bool hiddenInLists;
            public int category;
            public bool anyDirectionAxis;
            public bool allowSameFrameMultiInput;
        }

        [System.Serializable]
        public class Maps : IEnumerable<IInputMap>
        {
            // NO POINTERS/INTERFACES, not serializable otherwise!
            public List<KeyboardMap> kbMaps = new List<KeyboardMap>();
            public List<GamepadMap> gpButtonMaps = new List<GamepadMap>();
            public List<JoystickMap> jsMaps = new List<JoystickMap>();

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

                foreach (var map in jsMaps)
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

                GamepadMap gpButtonMap = map as GamepadMap;
                if (gpButtonMap != null)
                    gpButtonMaps.Add(gpButtonMap);

                JoystickMap jsButtonMap = map as JoystickMap;
                if (jsButtonMap != null)
                    jsMaps.Add(jsButtonMap);
            }

            public delegate bool CheckInputFn(IInputMap map);
            public bool CheckInputOnAllMapsGCFree(InputDeviceBase.CheckInputFn inputFn)
            {
                // Iterate over these with array indexing to avoid GC allocs from enumerator
                for (int inputMapIndex = 0; inputMapIndex < kbMaps.Count; ++inputMapIndex)
                {
                    IInputMap map = kbMaps[inputMapIndex];
                    if (map != null && !map.IsEmpty)
                    {
                        if (inputFn(map))
                            return true;
                    }
                }

                for (int inputMapIndex = 0; inputMapIndex < gpButtonMaps.Count; ++inputMapIndex)
                {
                    IInputMap map = gpButtonMaps[inputMapIndex];
                    if (map != null && !map.IsEmpty)
                    {
                        if (inputFn(map))
                            return true;
                    }
                }

                for (int inputMapIndex = 0; inputMapIndex < jsMaps.Count; ++inputMapIndex)
                {
                    IInputMap map = jsMaps[inputMapIndex];
                    if (map != null && !map.IsEmpty)
                    {
                        if (inputFn(map))
                            return true;
                    }
                }

                return false;
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
            // Handle previous versions of save data that didn't have gamepad maps
            if (saveData.input.kbMaps != null && saveData.input.kbMaps.Count > 0)
            {
                inputMaps.kbMaps.Clear();
                foreach(var map in saveData.input.kbMaps)
                {
                    inputMaps.kbMaps.Add(map.Clone() as KeyboardMap);
                }
            }

            if (saveData.input.gpButtonMaps != null && saveData.input.gpButtonMaps.Count > 0)
            {
                inputMaps.gpButtonMaps.Clear();
                foreach (var map in saveData.input.gpButtonMaps)
                {
                    inputMaps.gpButtonMaps.Add(map.Clone() as GamepadMap);
                }
            }

            if (saveData.input.jsMaps != null && saveData.input.jsMaps.Count > 0)
            {
                inputMaps.jsMaps.Clear();
                foreach (var map in saveData.input.jsMaps)
                {
                    inputMaps.jsMaps.Add(map.Clone() as JoystickMap);
                }
            }
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
                if(inputMap.HasConflict(map, this.properties))
                {
                    return true;
                }
            }

            return false;
        }

        public List<IInputMap> GetMapsForDevice(IInputDevice device)
        {
            List<IInputMap> deviceMaps = new List<IInputMap>();

            foreach (IInputMap map in inputMaps)
            {
                if (map.IsCompatibleWithDevice(device))
                    deviceMaps.Add(map);
            }

            if (deviceMaps.Count <= 0)
            {
                // Make some defaults
                IInputMap newMap = device.MakeDefaultMap();
                inputMaps.Add(newMap);
                deviceMaps.Add(newMap);
            }

            return deviceMaps;
        }

        public void RemoveMapsForDevice(IInputDevice device)
        {
            for (int i = inputMaps.kbMaps.Count - 1; i >= 0; --i)
            {
                if (inputMaps.kbMaps[i].IsCompatibleWithDevice(device))
                    inputMaps.kbMaps.RemoveAt(i);
            }

            for (int i = inputMaps.gpButtonMaps.Count - 1; i >= 0; --i)
            {
                if (inputMaps.gpButtonMaps[i].IsCompatibleWithDevice(device))
                    inputMaps.gpButtonMaps.RemoveAt(i);
            }

            for (int i = inputMaps.jsMaps.Count - 1; i >= 0; --i)
            {
                if (inputMaps.jsMaps[i].IsCompatibleWithDevice(device))
                {
                    inputMaps.jsMaps.RemoveAt(i);
                }
            }
        }

        #region Input Queries

        public bool GetInputDown(IList<IInputDevice> devices)
        {
            for (int i = 0; i < devices.Count; ++i)
            {
                IInputDevice device = devices[i];

                if (inputMaps.CheckInputOnAllMapsGCFree(device.GetInputDownDel))
                {
                    return true;
                }
            }

            return false;
        }

        public bool GetInputUp(IList<IInputDevice> devices)
        {
            for (int i = 0; i < devices.Count; ++i)
            {
                IInputDevice device = devices[i];

                if (inputMaps.CheckInputOnAllMapsGCFree(device.GetInputUpDel))
                {
                    return true;
                }
            }

            return false;
        }

        public bool GetInput(IList<IInputDevice> devices)
        {
            for (int i = 0; i < devices.Count; ++i)
            {
                IInputDevice device = devices[i];

                if (inputMaps.CheckInputOnAllMapsGCFree(device.GetInputDel))
                {
                    return true;
                }
            }

            return false;
        }

        public float GetAxis(IList<IInputDevice> devices)
        {
            float? value = GetAxisMaybe(devices);
            return value.HasValue ? value.Value : 0;
        }

        public float? GetAxisMaybe(IList<IInputDevice> devices)
        {
            for (int i = 0; i < devices.Count; ++i)
            {
                IInputDevice device = devices[i];

                foreach (var map in inputMaps)
                {
                    if (map != null && !map.IsEmpty)
                    {
                        var value = device.GetAxis(map);
                        if (value.HasValue)
                        {
                            return value.Value;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        public IInputMap GetFirstActiveInputMap()
        {
            foreach(IInputMap map in inputMaps)
            {
                if (!map.IsEmpty)
                    return map;
            }

            return null;
        }
    }
}
