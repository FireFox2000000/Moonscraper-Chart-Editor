// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using GuitarInput;
using DrumsInput;

public class Indicators : MonoBehaviour {
    const int FRET_COUNT = 6;

    [SerializeField]
    GameObject[] indicatorParents = new GameObject[FRET_COUNT];
    [SerializeField]
    GameObject[] indicators = new GameObject[FRET_COUNT];
    [SerializeField]
    CustomFretManager[] customIndicators = new CustomFretManager[FRET_COUNT];
    [SerializeField]
    Color[] defaultStikelineFretColors;
    [SerializeField]
    Color[] ghlStikelineFretColors;
    [SerializeField]
    GHLHitAnimation[] ghlCustomFrets;

    [HideInInspector]
    public HitAnimation[] animations;

    SpriteRenderer[] fretRenders;

    readonly System.Collections.Generic.Dictionary<Note.GuitarFret, bool> bannedFretInputs = new System.Collections.Generic.Dictionary<Note.GuitarFret, bool>()
    {
        {   Note.GuitarFret.Open, true },
    };

    readonly System.Collections.Generic.Dictionary<Note.DrumPad, bool> bannedDrumPadInputs = new System.Collections.Generic.Dictionary<Note.DrumPad, bool>()
    {
        {   Note.DrumPad.Kick, true },
    };

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

        UpdateStrikerColors();
        SetStrikerPlacement();

        EventsManager.onChartReloadEventList.Add(UpdateStrikerColors);
        EventsManager.onChartReloadEventList.Add(SetStrikerPlacement);
        EventsManager.onChartReloadEventList.Add(Activate2D3DSwitch);
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
    void Update () {

        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot)
        {
            GamepadInput input = ChartEditor.GetInstance().inputManager.mainGamepad;
            Chart.GameMode gameMode = ChartEditor.GetInstance().currentChart.gameMode;

            if (gameMode == Chart.GameMode.Drums)
            {
                foreach (Note.DrumPad drumPad in System.Enum.GetValues(typeof(Note.DrumPad)))
                {
                    if (bannedDrumPadInputs.ContainsKey(drumPad))
                        continue;

                    if (input.GetPadInputControllerOrKeyboard(drumPad))
                        animations[(int)drumPad].Press();
                    else
                        animations[(int)drumPad].Release();
                }
            }
            else
            {
                foreach (Note.GuitarFret fret in System.Enum.GetValues(typeof(Note.GuitarFret)))
                {
                    if (bannedFretInputs.ContainsKey(fret))
                        continue;

                    if (input.GetFretInputControllerOrKeyboard(fret))
                        animations[(int)fret].Press();
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

    public void UpdateStrikerColors()
    {
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
            Color[] colors = Globals.ghLiveMode ? ghlStikelineFretColors : defaultStikelineFretColors;
            for (int i = 0; i < colors.Length; ++i)
            {
                fretRenders[i * 2].color = colors[i];
                fretRenders[i * 2 + 1].color = colors[i];
            }
        }
    }

    public void SetStrikerPlacement()
    {
        int range = indicatorParents.Length;

        for (int i = 0; i < range; ++i)
        {
            int number = i;
            if (GameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip)
            {
                number = range - (number + 1);
                if (!Globals.ghLiveMode)
                    number -= 1;
            }

            float xPos = NoteController.CHART_CENTER_POS + number * NoteController.positionIncrementFactor + NoteController.noteObjectPositionStartOffset;
            indicatorParents[i].transform.position = new Vector3(xPos, indicatorParents[i].transform.position.y, indicatorParents[i].transform.position.z);

            bool inHighwayRange = Mathf.Abs(xPos) <= NoteController.CHART_CENTER_POS + Mathf.Abs(NoteController.noteObjectPositionStartOffset);

            indicatorParents[i].SetActive(inHighwayRange);
        }
    }

    void Activate2D3DSwitch()
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
