using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public GamepadInput mainGamepad = new GamepadInput();
	
	// Update is called once per frame
	void Update () {
        mainGamepad.Update(ChartEditor.hasFocus);
    }
}
