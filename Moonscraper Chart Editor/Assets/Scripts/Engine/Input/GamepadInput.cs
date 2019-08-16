using UnityEngine;
using XInputDotNetPure;

public class GamepadInput {
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
    }

    int? m_padIndex;

    // Input device
    GamePadState? m_currentGamepadStateMaybe;
    GamePadState? m_previousGamepadStateMaybe;

    public GamepadInput(int? padIndex = null)
    {
        m_padIndex = padIndex;
    }

    public void Update(bool hasFocus)
    {
        m_previousGamepadStateMaybe = m_currentGamepadStateMaybe;
        m_currentGamepadStateMaybe = null;

        if (hasFocus)
        {
            if (m_padIndex != null)
            {
                GamePadState testState = GamePad.GetState((PlayerIndex)m_padIndex);
                if (testState.IsConnected)
                {
                    m_currentGamepadStateMaybe = testState;
                }
            }
            // Get any input
            else
            {
                for (int i = 0; i < 1; ++i)
                {
                    GamePadState testState = GamePad.GetState((PlayerIndex)i);
                    if (testState.IsConnected)
                    {
                        m_currentGamepadStateMaybe = testState;
                        break;
                    }
                }
            }
        }
    }

    public bool connected
    {
        get
        {
            return m_currentGamepadStateMaybe != null;
        }
    }

    public float GetAxis(Axis axis)
    {
        if (m_currentGamepadStateMaybe == null)
            return 0;

        GamePadState gamePadState = (GamePadState)m_currentGamepadStateMaybe;

        switch (axis)
        {
            case (Axis.LeftStickX):
                return gamePadState.ThumbSticks.Left.X;

            case (Axis.LeftStickY):
                return gamePadState.ThumbSticks.Left.Y;

            case (Axis.RightStickX):
                return gamePadState.ThumbSticks.Right.X;

            case (Axis.RightStickY):
                return gamePadState.ThumbSticks.Right.Y;

            default:
                Debug.LogError("Axis input not handled");
                break;
        }

        return 0;
    }

    public bool GetButton(Button button)
    {
        return GetButton(button, m_currentGamepadStateMaybe);
    }

    public bool GetButtonPressed(Button button)
    {
        return GetButton(button) && !GetButton(button, m_previousGamepadStateMaybe);
    }

    public bool GetButtonReleased(Button button)
    {
        return !GetButton(button) && GetButton(button, m_previousGamepadStateMaybe);
    }

    // Device specific
    bool GetButton(Button button, GamePadState? gamepadStateMaybe)
    {
        if (gamepadStateMaybe == null)
            return false;

        GamePadState gamepadState = (GamePadState)gamepadStateMaybe;
        const ButtonState PRESSED_STATE = ButtonState.Pressed;

        switch (button)
        {
            case (Button.A):
                return gamepadState.Buttons.A == PRESSED_STATE;

            case (Button.B):
                return gamepadState.Buttons.B == PRESSED_STATE;

            case (Button.X):
                return gamepadState.Buttons.X == PRESSED_STATE;

            case (Button.Y):
                return gamepadState.Buttons.Y == PRESSED_STATE;

            case (Button.LB):
                return gamepadState.Buttons.LeftShoulder == PRESSED_STATE;

            case (Button.RB):
                return gamepadState.Buttons.RightShoulder == PRESSED_STATE;

            case (Button.L3):
                return gamepadState.Buttons.LeftStick == PRESSED_STATE;

            case (Button.R3):
                return gamepadState.Buttons.RightStick == PRESSED_STATE;

            case (Button.Start):
                return gamepadState.Buttons.Start == PRESSED_STATE;

            case (Button.Select):
                return gamepadState.Buttons.Back == PRESSED_STATE;

            case (Button.DPadUp):
                return gamepadState.DPad.Up == PRESSED_STATE;

            case (Button.DPadDown):
                return gamepadState.DPad.Down == PRESSED_STATE;

            case (Button.DPadLeft):
                return gamepadState.DPad.Left == PRESSED_STATE;

            case (Button.DPadRight):
                return gamepadState.DPad.Right == PRESSED_STATE;

            default:
                Debug.LogError("Unhandled input button type " + button);
                break;
        }

        return false;
    }
}
