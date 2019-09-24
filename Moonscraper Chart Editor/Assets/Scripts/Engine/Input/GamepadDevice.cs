using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDL2;

namespace MSE
{
    namespace Input
    {
        public class GamepadDevice : IInputDevice
        {
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

            public IInputMap GetCurrentInput()
            {
                foreach (Button button in EnumX<Button>.Values)
                {
                    if (GetButton(button))
                    {
                        return new GamepadButtonMap() { button };
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

            public float GetAxis(Axis axis)
            {
                var gamePadState = GetCurrentGamepadState();

                return gamePadState.axisValues[axis];
            }

            void FlipGamepadStateBuffer()
            {
                ++gamepadStateCurrentBufferIndex;
                if (gamepadStateCurrentBufferIndex > 1)
                    gamepadStateCurrentBufferIndex = 0;
            }

            ref GamepadState GetCurrentGamepadState()
            {
                return ref statesDoubleBuffer[gamepadStateCurrentBufferIndex];
            }

            ref GamepadState GetPreviousGamepadState()
            {
                int previousBufferIndex = gamepadStateCurrentBufferIndex + 1;
                if (previousBufferIndex > 1)
                    previousBufferIndex = 0;

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
                    gamepadState.axisValues[axis] = (float)axisValue / short.MaxValue;

                    // Todo, deadzones
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

            public bool GetInputDown(IInputMap inputMap)
            {
                GamepadButtonMap map = inputMap as GamepadButtonMap;
                if (map != null)
                {
                    foreach (var button in map.buttons)
                    {
                        if (!GetButtonPressed(button))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public bool GetInputUp(IInputMap inputMap)
            {
                GamepadButtonMap map = inputMap as GamepadButtonMap;
                if (map != null)
                {
                    foreach (var button in map.buttons)
                    {
                        if (!GetButtonReleased(button))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public bool GetInput(IInputMap inputMap)
            {
                GamepadButtonMap map = inputMap as GamepadButtonMap;
                if (map != null)
                {
                    foreach (var button in map.buttons)
                    {
                        if (!GetButton(button))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }
    }
}