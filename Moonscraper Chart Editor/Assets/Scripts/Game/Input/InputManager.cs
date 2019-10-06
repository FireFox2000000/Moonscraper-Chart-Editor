using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE.Input;

using SDL2;
using System;

[UnitySingleton(UnitySingletonAttribute.Type.LoadedFromResources, false, "Prefabs/InputManager")]
public class InputManager : UnitySingleton<InputManager>
{
    public InputConfig inputPropertiesConfig;
    [SerializeField]
    TextAsset defaultControlsJson;
    [HideInInspector]
    MSChartEditorInput.MSChartEditorActionContainer _defaultControls;
    public MSChartEditorInput.MSChartEditorActionContainer defaultControls
    {
        get
        {
            if (_defaultControls == null)
            {
                _defaultControls = JsonUtility.FromJson<MSChartEditorInput.MSChartEditorActionContainer>(defaultControlsJson.text);
                _defaultControls.LoadFromSaveData(_defaultControls);
            }

            return _defaultControls;
        }
    }
    public MSE.Event<IInputDevice> disconnectEvent = new MSE.Event<IInputDevice>();

    public List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    public List<GamepadDevice> controllers = new List<GamepadDevice>();
    public List<JoystickDevice> joysticks = new List<JoystickDevice>();

    private void Start()
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_JOYSTICK) < 0)
        {
            Debug.LogError("SDL could not initialise! SDL Error: " + SDL.SDL_GetError());
        }
        else
        {
            Debug.Log("Successfully initialised SDL");

            int connectedJoysticks = SDL.SDL_NumJoysticks();
        }
    }

    // Update is called once per frame
    void Update () {
        SDL.SDL_Event sdlEvent;
        while (SDL.SDL_PollEvent(out sdlEvent) > 0)
        {
            switch (sdlEvent.type)
            {
                case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                    {
                        int index = sdlEvent.jdevice.which;
                        if (SDL.SDL_IsGameController(sdlEvent.jdevice.which) == SDL.SDL_bool.SDL_TRUE)
                        {
                            OnControllerConnect(index);
                        }
                        else
                        {
                            OnJoystickConnect(index);
                        }
                        break;
                    }
                case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                    {
                        IntPtr removedController = SDL.SDL_GameControllerFromInstanceID(sdlEvent.jdevice.which);

                        if (removedController != IntPtr.Zero)
                        {
                            OnControllerDisconnect(sdlEvent.jdevice.which);
                        }
                        else
                        {
                            OnJoystickDisconnect(sdlEvent.jdevice.which);
                        }
                        break;
                    }

                default: break;
            }
        }

        foreach(GamepadDevice gamepad in controllers)
        {
            gamepad.Update(ChartEditor.hasFocus);
        }

        foreach (JoystickDevice joystick in joysticks)
        {
            joystick.Update(ChartEditor.hasFocus);
        }
    }

    void OnControllerConnect(int index)
    {
        IntPtr gameController = SDL.SDL_GameControllerOpen(index);
        if (gameController != IntPtr.Zero)
        {
            Debug.Log("Added controller device " + index);

            GamepadDevice gamepad = new GamepadDevice(gameController);
            controllers.Insert(index, gamepad);
            devices.Add(gamepad);
        }
        else
        {
            Debug.LogError("Failed to get SDL Game Controller address " + index + ". " + SDL.SDL_GetError());
        }
    }

    void OnJoystickConnect(int index)
    {
        IntPtr joystick = SDL.SDL_JoystickOpen(index);
        if (joystick != IntPtr.Zero)
        {
            Debug.Log("Added joystick device " + index);

            JoystickDevice gamepad = new JoystickDevice(joystick);
            joysticks.Insert(index, gamepad);
            devices.Add(gamepad);
        }
        else
        {
            Debug.LogError("Failed to get SDL Joystick address " + index + ". " + SDL.SDL_GetError());
        }
    }

    void OnControllerDisconnect(int instanceId)
    {
        Debug.Log("Removed controller device " + instanceId);

        IntPtr removedController = SDL.SDL_GameControllerFromInstanceID(instanceId);

        Debug.Assert(removedController != IntPtr.Zero);

        for (int i = 0; i < controllers.Count; ++i)
        {
            if (controllers[i].sdlHandle == removedController)
            {
                IInputDevice device = controllers[i];

                controllers[i].Disconnect();
                SDL.SDL_GameControllerClose(removedController);

                Debug.Assert(devices.Remove(controllers[i]));
                controllers.RemoveAt(i);
                disconnectEvent.Fire(device);

                return;
            }
        }

        Debug.Assert(false, "Unable to find controller " + instanceId + " to remove");
    }

    void OnJoystickDisconnect(int instanceId)
    {
        Debug.Log("Removed joystick device " + instanceId);

        IntPtr removedController = SDL.SDL_JoystickFromInstanceID(instanceId);

        Debug.Assert(removedController != IntPtr.Zero);

        for (int i = 0; i < joysticks.Count; ++i)
        {
            if (joysticks[i].sdlHandle == removedController)
            {
                IInputDevice device = joysticks[i];

                joysticks[i].Disconnect();
                SDL.SDL_JoystickClose(removedController);

                Debug.Assert(devices.Remove(joysticks[i]));
                joysticks.RemoveAt(i);
                disconnectEvent.Fire(device);

                return;
            }
        }

        Debug.Assert(false, "Unable to find joystick " + instanceId + " to remove");
    }

    private void OnApplicationQuit()
    {
        foreach(GamepadDevice gamepad in controllers)
        {
            SDL.SDL_GameControllerClose(gamepad.sdlHandle);
        }

        controllers.Clear();

        Debug.Log("Disposing SDL");
        SDL.SDL_Quit();
    }
}
