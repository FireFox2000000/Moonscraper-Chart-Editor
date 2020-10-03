// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using SDL2;

namespace MoonscraperEngine.Input
{
    public class GamepadDevice : InputDeviceBase, IInputDevice
    {
        // Sensitivity settings
        const float kAxisDeadzoneThreshold = 0.2f;
        const float kRebindIntendedInputThreshold = 0.5f;
        const float kRebindIntendedDeltaInputThreshold = 0.1f;

        public IntPtr sdlHandle { get; private set; }
        GamepadState[] statesDoubleBuffer = new GamepadState[2];
        int gamepadStateCurrentBufferIndex = 0;

        string deviceName;

        public enum Button
        {
            A,
            B,
            X,
            Y,
            LB,
            RB,
            R3,
            L3,

            Start,
            Select,

            DPadUp,
            DPadDown,
            DPadLeft,
            DPadRight,
        }

        public enum Axis
        {
            LeftStickX,
            LeftStickY,

            RightStickX,
            RightStickY,

            LT,
            RT,
        }

        public enum AxisDir
        {
            Any,
            Positive,
            Negative,
        }

        static GamepadState EmptyState = new GamepadState() { buttonsDown = new EnumLookupTable<Button, bool>(), axisValues = new EnumLookupTable<Axis, float>() };

        public GamepadDevice(IntPtr sdlHandle)
        {
            this.sdlHandle = sdlHandle;

            deviceName = SDL.SDL_GameControllerName(sdlHandle);

            statesDoubleBuffer[0] = new GamepadState() { buttonsDown = new EnumLookupTable<Button, bool>(), axisValues = new EnumLookupTable<Axis, float>() };
            statesDoubleBuffer[1] = new GamepadState() { buttonsDown = new EnumLookupTable<Button, bool>(), axisValues = new EnumLookupTable<Axis, float>() };
        }

        ~GamepadDevice()
        {
            sdlHandle = IntPtr.Zero;
        }


        public void Update(bool hasFocus)
        {
            FlipGamepadStateBuffer();

            if (hasFocus)
            {
                GetState(ref GetCurrentGamepadState());
            }
        }

        public bool Connected { get { return sdlHandle != IntPtr.Zero; } }

        public DeviceType Type => DeviceType.Gamepad;

        public IInputMap GetCurrentInput(InputAction.Properties properties)
        {
            // Check buttons
            {
                bool allowMultiInput = properties.allowSameFrameMultiInput;
                List<Button> buttons = new List<Button>();

                foreach (Button button in EnumX<Button>.Values)
                {
                    if (GetButton(button))
                    {
                        buttons.Add(button);

                        if (!allowMultiInput)
                            break;
                    }
                }

                if (buttons.Count > 0)
                {
                    return new GamepadMap() { buttons };
                }
            }

            foreach (Axis axis in EnumX<Axis>.Values)
            {
                float axisVal = GetAxis(axis);
                float previousAxisVal = GetPreviousAxis(axis);

                if (Mathf.Abs(axisVal - previousAxisVal) > kRebindIntendedDeltaInputThreshold && Mathf.Abs(axisVal) > kRebindIntendedInputThreshold)
                {
                    AxisDir dir = properties.anyDirectionAxis ? AxisDir.Any : 
                        (axisVal > 0 ? AxisDir.Positive : AxisDir.Negative);

                    return new GamepadMap() { { axis, dir } };
                }
            }

            return null;
        }

        public string GetDeviceName()
        {
            return deviceName;
        }

        public bool GetButton(Button button)
        {
            return GetButton(button, GetCurrentGamepadState());
        }

        public bool GetButtonPressed(Button button)
        {
            return GetButton(button) && !GetButton(button, GetPreviousGamepadState());
        }

        public bool GetButtonReleased(Button button)
        {
            return !GetButton(button) && GetButton(button, GetPreviousGamepadState());
        }

        public void Disconnect()
        {
            sdlHandle = IntPtr.Zero;
        }

        public float GetAxis(Axis axis)
        {
            var gamePadState = GetCurrentGamepadState();

            return gamePadState.axisValues[axis];
        }

        float GetPreviousAxis(Axis axis)
        {
            var gamePadState = GetPreviousGamepadState();

            return gamePadState.axisValues[axis];
        }

        void FlipGamepadStateBuffer()
        {
            gamepadStateCurrentBufferIndex ^= 1;
        }

        ref GamepadState GetCurrentGamepadState()
        {
            return ref statesDoubleBuffer[gamepadStateCurrentBufferIndex];
        }

        ref GamepadState GetPreviousGamepadState()
        {
            int previousBufferIndex = gamepadStateCurrentBufferIndex ^ 1;
            return ref statesDoubleBuffer[previousBufferIndex];
        }

        struct GamepadState
        {
            public EnumLookupTable<Button, bool> buttonsDown;
            public EnumLookupTable<Axis, float> axisValues;
        }

        bool GetButton(Button button, in GamepadState state)
        {
            return state.buttonsDown[button];
        }

        void GetState(ref GamepadState gamepadState)
        {
            foreach(Button button in EnumX<Button>.Values)
            {
                SDL.SDL_GameControllerButton sdlButton = GetSDLButtonForButton(button);
                gamepadState.buttonsDown[button] = SDL.SDL_GameControllerGetButton(sdlHandle, sdlButton) != 0;
            }

            foreach (Axis axis in EnumX<Axis>.Values)
            {
                SDL.SDL_GameControllerAxis sdlAxis = GetSDLAxisForAxis(axis);
                short axisValue = SDL.SDL_GameControllerGetAxis(sdlHandle, sdlAxis);
                float rawValue = (float)axisValue / short.MaxValue;

                // Deadzones
                if (Mathf.Abs(rawValue) < kAxisDeadzoneThreshold)
                {
                    rawValue = 0;
                }

                gamepadState.axisValues[axis] = rawValue;
            }
        }

        SDL.SDL_GameControllerButton GetSDLButtonForButton(Button button)
        {
            switch (button)
            {
                case Button.A: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A;
                case Button.B: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B;
                case Button.X: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X;
                case Button.Y: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y;
                case Button.LB: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER;
                case Button.RB: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER;

                case Button.R3: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK;
                case Button.L3: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK;

                case Button.Start: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START;
                case Button.Select: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK;

                case Button.DPadUp: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP;
                case Button.DPadDown: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN;
                case Button.DPadLeft: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT;
                case Button.DPadRight: return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT;

                default: break;
            }

            return SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_INVALID;
        }

        SDL.SDL_GameControllerAxis GetSDLAxisForAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.LeftStickX: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX;
                case Axis.LeftStickY: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY;
                case Axis.RightStickX: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX;
                case Axis.RightStickY: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY;
                case Axis.LT: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT;
                case Axis.RT: return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT;

                default: break;
            }

            return SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_INVALID;
        }

        public override bool GetInputDown(IInputMap inputMap)
        {
            GamepadMap map = inputMap as GamepadMap;
            if (map != null)
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButtonPressed(button))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    if (GetPreviousAxis(axis.axis) == 0)
                    {
                        float axisVal = GetAxis(axis.axis);

                        if (axisVal == 0 || (axis.dir == AxisDir.Positive && axisVal < 0) || (axis.dir == AxisDir.Negative && axisVal > 0))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override bool GetInputUp(IInputMap inputMap)
        {
            GamepadMap map = inputMap as GamepadMap;
            if (map != null)
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButtonReleased(button))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    if (GetAxis(axis.axis) == 0)
                    {
                        float axisVal = GetPreviousAxis(axis.axis);

                        if (axisVal == 0 || (axis.dir == AxisDir.Positive && axisVal < 0) || (axis.dir == AxisDir.Negative && axisVal > 0))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override bool GetInput(IInputMap inputMap)
        {
            GamepadMap map = inputMap as GamepadMap;
            if (map != null)
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButton(button))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    float axisVal = GetAxis(axis.axis);

                    if (axisVal == 0 || (axis.dir == AxisDir.Positive && axisVal < 0) || (axis.dir == AxisDir.Negative && axisVal > 0))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public float? GetAxis(IInputMap inputMap)
        {
            GamepadMap map = inputMap as GamepadMap;
            if (map != null)
            {
                foreach (var axis in map.axes)
                {
                    // We have an axis, use it
                    float axisVal = GetAxis(axis.axis);
                    return axisVal;
                }

                foreach (var button in map.buttons)
                {
                    if (!GetButton(button))
                    {
                        return 0;
                    }
                }


                return map.buttons.Count > 0 ? (float?)1 : null;
            }

            return null;
        }

        public IInputMap MakeDefaultMap()
        {
            return new GamepadMap();
        }
    }
}