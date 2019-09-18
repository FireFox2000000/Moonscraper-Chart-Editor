using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSE.Input;

[UnitySingleton(UnitySingletonAttribute.Type.LoadedFromResources, false, "Prefabs/InputManager")]
public class InputManager : UnitySingleton<InputManager>
{
    public InputConfig inputPropertiesConfig;

    public GamepadInput mainGamepad = new GamepadInput();
    public List<IInputDevice> devices = new List<IInputDevice>() { new KeyboardDevice() };

    // Update is called once per frame
    void Update () {
        mainGamepad.Update(ChartEditor.hasFocus);
    }
}
