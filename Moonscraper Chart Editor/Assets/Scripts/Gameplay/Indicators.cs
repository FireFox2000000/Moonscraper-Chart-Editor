// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define GAMEPAD

using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class Indicators : MonoBehaviour {
    const int FRET_COUNT = 5;

    [SerializeField]
    GameObject[] indicators = new GameObject[FRET_COUNT];
    [SerializeField]
    GameObject[] customIndicators = new GameObject[FRET_COUNT];
    [SerializeField]
    Color[] defaultStikelineFretColors;
    [HideInInspector]
    public HitAnimation[] animations = new HitAnimation[FRET_COUNT];

    SpriteRenderer[] fretRenders = new SpriteRenderer[FRET_COUNT * 2];

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

        for (int i = 0; i < indicators.Length; ++i)
        {
            fretRenders[i * 2] = indicators[i].GetComponent<SpriteRenderer>();
            fretRenders[i * 2 + 1] = indicators[i].transform.parent.GetComponent<SpriteRenderer>();
        }
    }

    // Update is called once per frame
    void Update () {
        if (Globals.drumMode)
        {
            for (int i = 0; i < defaultStikelineFretColors.Length; ++i)
            {
                int color = i + 1;
                if (color >= defaultStikelineFretColors.Length)
                    color = 0;

                fretRenders[i * 2].color = defaultStikelineFretColors[color];
                fretRenders[i * 2 + 1].color = defaultStikelineFretColors[color];
            }
        }
        else
        {
            for (int i = 0; i < defaultStikelineFretColors.Length; ++i)
            {
                fretRenders[i * 2].color = defaultStikelineFretColors[i];
                fretRenders[i * 2 + 1].color = defaultStikelineFretColors[i];
            }
        }
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
