// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimesignatureController : SongObjectController {

    public TimeSignature ts { get { return (TimeSignature)songObject; } set { Init(value, this); } }
    public Text tsText;
    public const float position = 1.5f;

    public override void UpdateSongObject()
    {
        if (ts.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, ts.worldYPosition, 0);

            tsText.text = ts.numerator.ToString() + "/" + ts.denominator.ToString();
        }
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
