
namespace MSE
{
    namespace Input
    {
        [System.Serializable]
        public class InputAction
        {
            public enum Device
            {
                Keyboard,
                //Gamepad,
            }

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

            public bool GetInputDown()
            {
                for (int i = 0; i < kbMaps.Length; ++i)
                {
                    KeyboardMap map = kbMaps[i];
                    if (map != null)
                    {
                        if (KeyboardMap.GetInputDown(map))
                            return true;
                    }
                }

                return false;
            }

            public bool GetInputUp()
            {
                for (int i = 0; i < kbMaps.Length; ++i)
                {
                    KeyboardMap map = kbMaps[i];
                    if (map != null)
                    {
                        if (KeyboardMap.GetInputUp(map))
                            return true;
                    }
                }

                return false;
            }

            public bool GetInput()
            {
                for (int i = 0; i < kbMaps.Length; ++i)
                {
                    KeyboardMap map = kbMaps[i];
                    if (map != null)
                    {
                        if (KeyboardMap.GetInput(map))
                            return true;
                    }
                }

                return false;
            }

            public IInputMap[] GetMapsForDevice(Device device)
            {
                switch (device)
                {
                    case Device.Keyboard: return kbMaps;
                    default: return null;
                }
            }
        }
    }
}
