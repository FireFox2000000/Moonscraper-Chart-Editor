// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDL2;

namespace MoonscraperEngine.Input
{
    public class JoystickDevice : InputDeviceBase, IInputDevice
    {
        // Sensitivity settings
        const float kAxisDeadzoneThreshold = 0.2f;
        const float kRebindIntendedInputThreshold = 0.5f;
        const float kRebindIntendedDeltaInputThreshold = 0.1f;

        public IntPtr sdlHandle { get; private set; }
        readonly string deviceName;
        public readonly string deviceId;
        public readonly JoystickType joystickType = JoystickType.Unknown;
        JoystickState[] statesDoubleBuffer = new JoystickState[2];
        int stateCurrentBufferIndex = 0;

        readonly int totalButtons = 0;
        readonly int totalAxis = 0;
        readonly int totalBalls = 0;
        readonly int totalHats = 0;

        public enum JoystickType
        {
            Unknown = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_UNKNOWN,
            GameController = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_GAMECONTROLLER,
            Wheel = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_WHEEL,
            ArcadeStick = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_ARCADE_STICK,
            FlightStick = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_FLIGHT_STICK,
            DancePad = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_DANCE_PAD,
            Guitar = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_GUITAR,
            DrumKit = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_DRUM_KIT,
            ArcadePad = SDL.SDL_JoystickType.SDL_JOYSTICK_TYPE_ARCADE_PAD
        }

        struct JoystickState
        {
            public bool[] buttonsDown;
            public float[] axisValues;
            public Vector2[] ballValues;
            public HatPosition[] hatPositions;
        }

        [Flags]
        public enum HatPosition
        {
            CENTERED = SDL.SDL_HAT_CENTERED,
            UP = SDL.SDL_HAT_UP,
            RIGHT = SDL.SDL_HAT_RIGHT,
            DOWN = SDL.SDL_HAT_DOWN,
            LEFT = SDL.SDL_HAT_LEFT,
            RIGHTUP = SDL.SDL_HAT_RIGHTUP,
            RIGHTDOWN = SDL.SDL_HAT_RIGHTDOWN,
            LEFTUP = SDL.SDL_HAT_LEFTUP,
            LEFTDOWN = SDL.SDL_HAT_LEFTDOWN,
        }

        public enum AxisDir
        {
            Any,
            Positive,
            Negative,
        }

        public JoystickDevice(IntPtr sdlHandle)
        {
            this.sdlHandle = sdlHandle;

            joystickType = (JoystickType)SDL.SDL_JoystickGetType(sdlHandle);
            deviceId = SDL.SDL_JoystickName(sdlHandle);
            deviceName = string.Format("{0} ({1})", deviceId, joystickType.ToString());

            totalButtons = SDL.SDL_JoystickNumButtons(sdlHandle);
            totalAxis = SDL.SDL_JoystickNumAxes(sdlHandle);
            totalBalls = SDL.SDL_JoystickNumBalls(sdlHandle);
            totalHats = SDL.SDL_JoystickNumHats(sdlHandle);

            for (int i = 0; i < statesDoubleBuffer.Length; ++i)
            {
                statesDoubleBuffer[i] = new JoystickState() {
                    buttonsDown = new bool[totalButtons],
                    axisValues = new float[totalAxis],
                    ballValues = new Vector2[totalBalls],
                    hatPositions = new HatPosition[totalHats],
                };
            }              
        }

        ~JoystickDevice()
        {
            sdlHandle = IntPtr.Zero;
        }

        public void Update(bool hasFocus)
        {
            FlipStateBuffer();

            if (hasFocus)
            {
                GetState(ref GetCurrentJoystickState());
            }
        }

        void FlipStateBuffer()
        {
            stateCurrentBufferIndex ^= 1;
        }

        ref JoystickState GetCurrentJoystickState()
        {
            return ref statesDoubleBuffer[stateCurrentBufferIndex];
        }

        ref JoystickState GetPreviousJoystickState()
        {
            int previousBufferIndex = stateCurrentBufferIndex ^ 1;
            return ref statesDoubleBuffer[previousBufferIndex];
        }

        void GetState(ref JoystickState joystickState)
        {
            for (int i = 0; i < joystickState.buttonsDown.Length; ++i)
            {
                joystickState.buttonsDown[i] = SDL.SDL_JoystickGetButton(sdlHandle, i) != 0;
            }

            for (int i = 0; i < joystickState.axisValues.Length; ++i)
            {
                short axisValue = SDL.SDL_JoystickGetAxis(sdlHandle, i);
                float rawValue = (float)axisValue / short.MaxValue;

                // Deadzones
                if (Mathf.Abs(rawValue) < kAxisDeadzoneThreshold)
                {
                    rawValue = 0;
                }

                joystickState.axisValues[i] = rawValue;
            }

            for (int i = 0; i < joystickState.hatPositions.Length; ++i)
            {
                joystickState.hatPositions[i] = (HatPosition)SDL.SDL_JoystickGetHat(sdlHandle, i);
            }

            // Todo, balls. Probably not supported for the current project
        }

        bool GetButton(int buttonIndex, in JoystickState state)
        {
            if (buttonIndex < state.buttonsDown.Length)
                return state.buttonsDown[buttonIndex];

            Debug.Assert(false); // Button index incorrect
            return false;
        }

        public bool GetButton(int button)
        {
            return GetButton(button, GetCurrentJoystickState());
        }

        public bool GetButtonPressed(int button)
        {
            return GetButton(button) && !GetButton(button, GetPreviousJoystickState());
        }

        public bool GetButtonReleased(int button)
        {
            return !GetButton(button) && GetButton(button, GetPreviousJoystickState());
        }

        public float GetAxis(int axis)
        {
            var joystickState = GetCurrentJoystickState();

            if (axis < joystickState.axisValues.Length)
            {
                return joystickState.axisValues[axis];
            }

            Debug.Assert(false); // Invalid axis index

            return 0;
        }

        float GetPreviousAxis(int axis)
        {
            var joystickState = GetPreviousJoystickState();

            if (axis < joystickState.axisValues.Length)
            {
                return joystickState.axisValues[axis];
            }

            Debug.Assert(false); // Invalid axis index

            return 0;
        }

        HatPosition GetHat(int hatIndex, in JoystickState state)
        {
            if (hatIndex < state.hatPositions.Length)
                return state.hatPositions[hatIndex];

            Debug.Assert(false); // Hat index incorrect
            return HatPosition.CENTERED;
        }

        public HatPosition GetHatPosition(int hat)
        {
            return GetHat(hat, GetCurrentJoystickState());
        }

        public bool GetHatPositionChanged(int hat)
        {
            return GetHat(hat, GetCurrentJoystickState()) != GetHat(hat, GetPreviousJoystickState());
        }

        public bool GetHatPositionEntered(int hat, HatPosition hatPosition)
        {
            return GetHatPosition(hat) == hatPosition && GetHat(hat, GetPreviousJoystickState()) != hatPosition;
        }

        public bool GetHatPositionExited(int hat, HatPosition hatPosition)
        {
            return GetHatPosition(hat) != hatPosition && GetHat(hat, GetPreviousJoystickState()) == hatPosition;
        }

        public bool Connected { get { return sdlHandle != IntPtr.Zero; } }

        public void Disconnect()
        {
            sdlHandle = IntPtr.Zero;
        }

        public string GetDeviceName()
        {
            return deviceName;
        }

        public DeviceType Type => DeviceType.Joystick;

        public IInputMap GetCurrentInput(InputAction.Properties properties)
        {
            // Check buttons
            {
                bool allowMultiInput = properties.allowSameFrameMultiInput;
                List<JoystickMap.ButtonConfig> buttons = new List<JoystickMap.ButtonConfig>();

                for (int i = 0; i < totalButtons; ++i)
                {
                    if (GetButton(i))
                    {
                        buttons.Add(new JoystickMap.ButtonConfig() { buttonIndex = i });

                        if (!allowMultiInput)
                            break;
                    }
                }

                if (buttons.Count > 0)
                {
                    return new JoystickMap(deviceId) { buttons };
                }
            }

            // Check axis
            {
                for (int i = 0; i < totalAxis; ++i)
                {
                    float axisVal = GetAxis(i);
                    float previousAxisVal = GetPreviousAxis(i);

                    if (Mathf.Abs(axisVal - previousAxisVal) > kRebindIntendedDeltaInputThreshold && Mathf.Abs(axisVal) > kRebindIntendedInputThreshold)
                    {
                        AxisDir dir = properties.anyDirectionAxis ? AxisDir.Any :
                            (axisVal > 0 ? AxisDir.Positive : AxisDir.Negative);

                        return new JoystickMap(deviceId) { { i, dir } };
                    }
                }
            }

            // Ball not supported, todo

            // Check hats
            {
                List<JoystickMap.HatConfig> hats = new List<JoystickMap.HatConfig>();

                for (int i = 0; i < totalHats; ++i)
                {
                    if (GetHatPositionChanged(i) && GetHatPosition(i) != HatPosition.CENTERED)
                    {
                        hats.Add(new JoystickMap.HatConfig() { hatIndex = i, position = GetHatPosition(i), });
                    }
                }

                if (hats.Count > 0)
                {
                    return new JoystickMap(deviceId) { hats };
                }
            }

            return null;
        }
            
        public override bool GetInput(IInputMap inputMap)
        {
            JoystickMap map = inputMap as JoystickMap;
            if (map != null && map.IsCompatibleWithDevice(this))
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButton(button.buttonIndex))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    float axisVal = GetAxis(axis.axisIndex);

                    if (axisVal == 0 || (axis.dir == AxisDir.Positive && axisVal < 0) || (axis.dir == AxisDir.Negative && axisVal > 0))
                    {
                        return false;
                    }
                }

                foreach (var ball in map.balls)
                {
                    // Ball not supported, todo
                    return false;
                }

                foreach (var hat in map.hats)
                {
                    if (GetHatPosition(hat.hatIndex) == hat.position)
                    {
                        return false;
                    }
                    return false;
                }

                return true;
            }

            return false;
        }

        public override bool GetInputDown(IInputMap inputMap)
        {
            JoystickMap map = inputMap as JoystickMap;
            if (map != null && map.IsCompatibleWithDevice(this))
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButtonPressed(button.buttonIndex))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    if (GetPreviousAxis(axis.axisIndex) == 0)
                    {
                        float axisVal = GetAxis(axis.axisIndex);

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

                foreach (var ball in map.balls)
                {
                    // Ball not supported, todo
                    return false;
                }

                foreach (var hat in map.hats)
                {
                    if (!GetHatPositionEntered(hat.hatIndex, hat.position))
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
            JoystickMap map = inputMap as JoystickMap;
            if (map != null && map.IsCompatibleWithDevice(this))
            {
                foreach (var button in map.buttons)
                {
                    if (!GetButtonReleased(button.buttonIndex))
                    {
                        return false;
                    }
                }

                foreach (var axis in map.axes)
                {
                    if (GetAxis(axis.axisIndex) == 0)
                    {
                        float axisVal = GetPreviousAxis(axis.axisIndex);

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

                foreach (var ball in map.balls)
                {
                    // Ball not supported, todo
                    return false;
                }

                foreach (var hat in map.hats)
                {
                    if (!GetHatPositionExited(hat.hatIndex, hat.position))
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
            JoystickMap map = inputMap as JoystickMap;
            if (map != null && map.IsCompatibleWithDevice(this))
            {
                foreach (var axis in map.axes)
                {
                    // We have an axis, use it
                    float axisVal = GetAxis(axis.axisIndex);
                    return axisVal;
                }

                foreach (var button in map.buttons)
                {
                    if (!GetButton(button.buttonIndex))
                    {
                        return 0;
                    }
                }

                if (map.buttons.Count > 0)
                {
                    return 1;
                }

                // Ball not supported, todo

                // Hat not supported, todo
                foreach (var hat in map.hats)
                {
                    if (GetHatPosition(hat.hatIndex) != hat.position)
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
            return new JoystickMap(deviceId);
        }
    }
}
