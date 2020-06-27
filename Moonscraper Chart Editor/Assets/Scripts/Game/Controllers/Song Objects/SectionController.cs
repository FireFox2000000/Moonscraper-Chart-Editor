// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using TMPro;
using MoonscraperChartEditor.Song;

public class SectionController : SongObjectController
{
    public Section section { get { return (Section)songObject; } set { Init(value, this); } }
    public const float position = 4.0f;
    public TextMeshPro sectionText;

    public override void UpdateSongObject()
    {
        if (section.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0);

            sectionText.text = section.title;
        }
    }
}
