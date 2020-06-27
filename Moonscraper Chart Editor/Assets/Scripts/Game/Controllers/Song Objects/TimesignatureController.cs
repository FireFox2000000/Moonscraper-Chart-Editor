// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using TMPro;
using MoonscraperChartEditor.Song;

public class TimesignatureController : SongObjectController {

    public TimeSignature ts { get { return (TimeSignature)songObject; } set { Init(value, this); } }
    public TextMeshPro tsText;
    public const float position = 1.5f;
    uint previousNumerator = 0, previousDenominator = 0;

    public override void UpdateSongObject()
    {
        if (ts.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0);

            if (previousNumerator != ts.numerator || previousDenominator != ts.denominator)
                UpdateDisplay();

            previousNumerator = ts.numerator;
            previousDenominator = ts.denominator;
        }
    }

    void UpdateDisplay()
    {
        tsText.text = ts.numerator.ToString() + "/" + ts.denominator.ToString();
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (ts.tick != 0)
        {
            base.OnSelectableMouseDrag();
        }
    }
}
