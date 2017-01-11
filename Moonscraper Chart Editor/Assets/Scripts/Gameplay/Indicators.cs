//#define GAMEPAD

using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class Indicators : MonoBehaviour {
    public GameObject[] indicators = new GameObject[5];
    GamePadState gamepad;

    // Update is called once per frame
    void Update () {
        gamepad = GamePad.GetState(0);

        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !Globals.bot)
        {
#if GAMEPAD
            if (gamepad.Buttons.A == ButtonState.Pressed)
                indicators[0].SetActive(true);
            else
                indicators[0].SetActive(false);

            if (gamepad.Buttons.B == ButtonState.Pressed)
                indicators[1].SetActive(true);
            else
                indicators[1].SetActive(false);

            if (gamepad.Buttons.Y == ButtonState.Pressed)
                indicators[2].SetActive(true);
            else
                indicators[2].SetActive(false);

            if (gamepad.Buttons.X == ButtonState.Pressed)
                indicators[3].SetActive(true);
            else
                indicators[3].SetActive(false);

            if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed)
                indicators[4].SetActive(true);
            else
                indicators[4].SetActive(false);
#else
            if (Input.GetButton("FretGreen"))
                indicators[0].SetActive(true);
            else
                indicators[0].SetActive(false);

            if (Input.GetButton("FretRed"))
                indicators[1].SetActive(true);
            else
                indicators[1].SetActive(false);

            if (Input.GetButton("FretYellow"))
                indicators[2].SetActive(true);
            else
                indicators[2].SetActive(false);

            if (Input.GetButton("FretBlue"))
                indicators[3].SetActive(true);
            else
                indicators[3].SetActive(false);

            if (Input.GetButton("FretOrange"))
                indicators[4].SetActive(true);
            else
                indicators[4].SetActive(false);
#endif
        }
        else
        {
            foreach (GameObject indicator in indicators)
                indicator.SetActive(false);
        }
    }
}
