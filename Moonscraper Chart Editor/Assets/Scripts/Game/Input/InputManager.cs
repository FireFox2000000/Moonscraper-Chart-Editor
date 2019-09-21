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

    public GamepadInput mainGamepad = new GamepadInput();
    public List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    public List<GamepadDevice> controllers = new List<GamepadDevice>();

    private void Start()
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER) < 0)
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
        mainGamepad.Update(ChartEditor.hasFocus);

        SDL.SDL_Event sdlEvent;
        while (SDL.SDL_PollEvent(out sdlEvent) > 0)
        {
            switch (sdlEvent.type)
            {
                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    {
                        int index = sdlEvent.jdevice.which;
                        OnControllerConnect(index);
                        break;
                    }

                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    {
                        int id = sdlEvent.jdevice.which;
                        OnControllerDisconnect(id);
                        break;
                    }

                default: break;
            }
        }

        foreach(GamepadDevice gamepad in controllers)
        {
            gamepad.Update();
        }
    }

    void OnControllerConnect(int index)
    {
        IntPtr gameController = SDL.SDL_GameControllerOpen(index);
        if (gameController != IntPtr.Zero)
        {
            Debug.Log("Added controller device " + index);
            SDL.SDL_GameControllerOpen(index);
            controllers.Insert(index, new GamepadDevice(gameController));
        }
        else
        {
            Debug.LogError("Failed to get SDL Game Controller address " + index + ". " + SDL.SDL_GetError());
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
                SDL.SDL_GameControllerClose(removedController);
                controllers.RemoveAt(i);
                break;
            }
        }

        Debug.Assert(false, "Unable to find controller " + instanceId + " to remove");
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
