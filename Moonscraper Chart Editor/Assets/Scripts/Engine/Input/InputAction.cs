
namespace MSE
{
    namespace Input
    {
        public class InputAction
        {
            public const int kMaxKeyboardMaps = 2;

            public bool rebindable;
            public KeyboardMap[] kbMaps = new KeyboardMap[kMaxKeyboardMaps];

            public InputAction(bool rebindable = true)
            {
                this.rebindable = rebindable;
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
        }
    }
}
