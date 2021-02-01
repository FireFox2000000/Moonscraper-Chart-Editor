// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine.Input;

using SDL2;
using System;

/// <summary>
/// Handles controller connection events and updates controller inputs
/// </summary>
[UnitySingleton(UnitySingletonAttribute.Type.LoadedFromResources, false, "Prefabs/InputManager")]
public class InputManager : UnitySingleton<InputManager>
{
    [SerializeField]
    TextAsset inputPropertiesJson;
    InputConfig _inputProperties;
    public InputConfig inputProperties
    {
        get
        {
            if (_inputProperties == null)
            {
                _inputProperties = new InputConfig();
                InputConfig.LoadFromTextAsset(inputPropertiesJson, _inputProperties);
            }

            return _inputProperties;
        }
    }

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
    public MoonscraperEngine.Event<IInputDevice> disconnectEvent = new MoonscraperEngine.Event<IInputDevice>();

    public List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    public List<GamepadDevice> controllers = new List<GamepadDevice>();
    public List<JoystickDevice> joysticks = new List<JoystickDevice>();

    private void Start()
    {
        Debug.Log("Initialising SDL input...");

        SDL.SDL_SetMainReady();

        Debug.Log("SDL input main ready");

        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_JOYSTICK) < 0)
        {
            Debug.LogError("SDL could not initialise! SDL Error: " + SDL.SDL_GetError());
        }
        else
        {
            Debug.Log("Successfully initialised SDL input");

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
            controllers.Add(gamepad);
            devices.Add(gamepad);

            Debug.Log("Controller count = " + controllers.Count);
            Debug.Log("Device count = " + devices.Count);
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
            joysticks.Add(gamepad);
            devices.Add(gamepad);

            Debug.Log("Joystick count = " + joysticks.Count);
            Debug.Log("Device count = " + devices.Count);
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

                bool deviceRemoved = devices.Remove(controllers[i]);
                Debug.Assert(deviceRemoved);
                controllers.RemoveAt(i);
                disconnectEvent.Fire(device);

                Debug.Log("Controller count = " + controllers.Count);
                Debug.Log("Device count = " + devices.Count);

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

                bool deviceRemoved = devices.Remove(joysticks[i]);
                Debug.Assert(deviceRemoved);
                joysticks.RemoveAt(i);
                disconnectEvent.Fire(device);

                Debug.Log("Joystick count = " + joysticks.Count);
                Debug.Log("Device count = " + devices.Count);

                return;
            }
        }

        Debug.Assert(false, "Unable to find joystick " + instanceId + " to remove");
    }

    public void Dispose()
    {
        foreach (GamepadDevice gamepad in controllers)
        {
            SDL.SDL_GameControllerClose(gamepad.sdlHandle);
        }

        controllers.Clear();
    }
}
