// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

namespace MoonscraperEngine.Input
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

        InputDeviceBase.CheckInputFn GetInputDownDel { get; }
        InputDeviceBase.CheckInputFn GetInputUpDel { get; }
        InputDeviceBase.CheckInputFn GetInputDel { get; }
    }

    public abstract class InputDeviceBase
    {
        public abstract bool GetInputDown(IInputMap map);
        public abstract bool GetInputUp(IInputMap map);
        public abstract bool GetInput(IInputMap map);

        public delegate bool CheckInputFn(IInputMap map);

        // Precreated delegate functions to avoid delegate GC allocs when checking for input later on
        public CheckInputFn GetInputDownDel { get; }
        public CheckInputFn GetInputUpDel { get; }
        public CheckInputFn GetInputDel { get; }

        public InputDeviceBase()
        {
            GetInputDownDel = GetInputDown;
            GetInputUpDel = GetInputUp;
            GetInputDel = GetInput;
        }
    }
}
