#define GAMEPAD

using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class Indicators : MonoBehaviour {
    [SerializeField]
    GameObject[] indicators = new GameObject[5];
    [SerializeField]
    GameObject[] customIndicators = new GameObject[5];
    [HideInInspector]
    public HitAnimation[] animations = new HitAnimation[5];

    void Start()
    {
        for(int i = 0; i < animations.Length; ++i)
        {
            if (customIndicators[i].activeSelf)
            {
                animations[i] = customIndicators[i].GetComponent<HitAnimation>();
                indicators[i].transform.parent.gameObject.SetActive(false);
            }
            else
                animations[i] = indicators[i].GetComponent<HitAnimation>();
        }
    }

    // Update is called once per frame
    void Update () {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !Globals.bot)
        {
#if GAMEPAD
            if (GameplayManager.gamepad != null)
            {
                GamePadState gamepad = (GamePadState)GameplayManager.gamepad;

                if (gamepad.Buttons.A == ButtonState.Pressed)
                    animations[0].Press();
                else
                    animations[0].Release();

                if (gamepad.Buttons.B == ButtonState.Pressed)
                    animations[1].Press();
                else
                    animations[1].Release();

                if (gamepad.Buttons.Y == ButtonState.Pressed)
                    animations[2].Press();
                else
                    animations[2].Release();

                if (gamepad.Buttons.X == ButtonState.Pressed)
                    animations[3].Press();
                else
                    animations[3].Release();

                if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed)
                    animations[4].Press();
                else
                    animations[4].Release();
            }
            else
            {
                // Keyboard controls
                for (int i = 0; i < 5; ++i)
                {
                    if (Input.GetKey((i + 1).ToString()))
                    {
                        animations[i].Press();
                    }
                    else if (!animations[i].running)
                        animations[i].Release();
                }
                /*
                foreach (GameObject indicator in indicators)
                {
                    indicator.SetActive(false);
                }*/
            }
#else

            for (int i = 0; i < 5; ++i)
            {
                if (Input.GetButton("Fret" + i))
                {
                    //indicators[i].SetActive(true);
                    animations[i].Press();
                }
                else if (!animations[i].running)
                    animations[i].Release();
                //indicators[i].SetActive(false);
            }
#endif
        }
        else
        {
            for (int i = 0; i < animations.Length; ++i)
            {
                if (!animations[i].running)
                    animations[i].Release();
            }
        }
    }
}
