﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;

public class Indicators : MonoBehaviour {
    const int FRET_COUNT = 6;

    [SerializeField]
    GameObject[] indicatorParents = new GameObject[FRET_COUNT];
    [SerializeField]
    GameObject[] indicators = new GameObject[FRET_COUNT];
    [SerializeField]
    CustomFretManager[] customIndicators = new CustomFretManager[FRET_COUNT];
    [SerializeField]
    GHLHitAnimation[] ghlCustomFrets;

    [SerializeField]
    LaneInfo laneInfo;

    [HideInInspector]
    public HitAnimation[] animations;

    SpriteRenderer[] fretRenders;

    readonly Dictionary<Note.GuitarFret, bool> bannedFretInputs = new Dictionary<Note.GuitarFret, bool>()
    {
        {   Note.GuitarFret.Open, true },
    };

    readonly Dictionary<Note.DrumPad, bool> bannedDrumPadInputs = new Dictionary<Note.DrumPad, bool>()
    {
        {   Note.DrumPad.Kick, true },
    };

    Dictionary<Chart.GameMode, int[]> paletteMaps;

    void Start()
    {
        animations = new HitAnimation[FRET_COUNT];
        fretRenders = new SpriteRenderer[FRET_COUNT * 2];

        SetAnimations();

        for (int i = 0; i < indicators.Length; ++i)
        {
            fretRenders[i * 2] = indicators[i].GetComponent<SpriteRenderer>();
            fretRenders[i * 2 + 1] = indicators[i].transform.parent.GetComponent<SpriteRenderer>();
        }

        UpdateStrikerColors(laneInfo.laneCount);
        SetStrikerPlacement(laneInfo.laneCount);

        ChartEditor.Instance.events.lanesChangedEvent.Register(UpdateStrikerColors);
        ChartEditor.Instance.events.lanesChangedEvent.Register(SetStrikerPlacement);
        ChartEditor.Instance.events.lanesChangedEvent.Register(Activate2D3DSwitch);
    }

    void SetAnimations()
    {
        for (int i = 0; i < animations.Length; ++i)
        {
            if (Globals.ghLiveMode)
            {
                if (ghlCustomFrets[i].canUse)
                {
                    animations[i] = ghlCustomFrets[i].gameObject.GetComponent<HitAnimation>();
                    indicators[i].transform.parent.gameObject.SetActive(false);
                }
                else
                    animations[i] = indicators[i].GetComponent<HitAnimation>();
            }
            else
            {
                if (customIndicators[i].gameObject.activeSelf)
                {
                    animations[i] = customIndicators[i].gameObject.GetComponent<HitAnimation>();
                    indicators[i].transform.parent.gameObject.SetActive(false);
                }
                else
                    animations[i] = indicators[i].GetComponent<HitAnimation>();
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        ChartEditor editor = ChartEditor.Instance;

        if (editor.currentState == ChartEditor.State.Playing && !GameSettings.bot)
        {
            Chart.GameMode gameMode = editor.currentChart.gameMode;
            LaneInfo laneInfo = editor.laneInfo;

            if (gameMode == Chart.GameMode.Drums)
            {
                foreach (Note.DrumPad drumPad in EnumX<Note.DrumPad>.Values)
                {
                    if (bannedDrumPadInputs.ContainsKey(drumPad))
                        continue;

                    bool lanePressed = false;
                    switch (GameSettings.drumsModeOptions)
                    {
                        case GameSettings.DrumModeOptions.ProDrums:
                            {
                                lanePressed = DrumsInput.GetTomPressedInput(drumPad, laneInfo) || DrumsInput.GetCymbalPressedInput(drumPad, laneInfo);
                                break;
                            }
                        default:
                            {
                                lanePressed = DrumsInput.GetPadPressedInput(drumPad, laneInfo);
                                break;
                            }
                    }

                    if (lanePressed)
                        animations[(int)drumPad].Press();
                    else
                        animations[(int)drumPad].Release();
                }
            }
            else
            {
                foreach (Note.GuitarFret fret in EnumX<Note.GuitarFret>.Values)
                {
                    if (bannedFretInputs.ContainsKey(fret))
                        continue;

                    if (GuitarInput.GetFretInput(fret))
                    {
                        animations[(int)fret].Press();
                    }
                    else
                        animations[(int)fret].Release();

                }
            }
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

    public void UpdateStrikerColors(in int laneCount)
    {
        Chart.GameMode gameMode = ChartEditor.Instance.currentGameMode;

        Color[] colours = laneInfo.laneColours;

        for (int i = 0; i < colours.Length; ++i)
        {
            fretRenders[i * 2].color = colours[i];
            fretRenders[i * 2 + 1].color = colours[i];
        }
    }

    public void SetStrikerPlacement(in int laneCount)
    {
        int range = indicatorParents.Length;
        bool lefyFlip = GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip;
        float chartCenterPos = NoteController.CHART_CENTER_POS;

        for (int i = 0; i < range; ++i)
        {
            if (i < laneCount)
            {
                float xPos = chartCenterPos + laneInfo.GetLanePosition(i, lefyFlip);
                indicatorParents[i].transform.position = new Vector3(xPos, indicatorParents[i].transform.position.y, indicatorParents[i].transform.position.z);
            }

            indicatorParents[i].SetActive(i < laneCount);
        }
    }

    void Activate2D3DSwitch(in int laneCount)
    {
        if (Globals.ghLiveMode)
        {
            foreach (CustomFretManager go in customIndicators)
            {
                go.gameObject.SetActive(false);
            }

            // Check if the sprites exist for 2D
            for (int i = 0; i < FRET_COUNT; ++i)
            {
                ghlCustomFrets[i].transform.parent.gameObject.SetActive(ghlCustomFrets[i].canUse);
                indicators[i].transform.parent.gameObject.SetActive(!ghlCustomFrets[i].canUse);
            }
        }
        else
        {
            foreach (GHLHitAnimation go in ghlCustomFrets)
            {
                go.gameObject.SetActive(false);
            }

            // Check if the sprites exist for 2D
            for (int i = 0; i < FRET_COUNT; ++i)
            {
                customIndicators[i].gameObject.SetActive(customIndicators[i].canUse);
                indicators[i].transform.parent.gameObject.SetActive(!customIndicators[i].canUse);
            }
        }

        SetAnimations();
    }
}
