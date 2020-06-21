// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public abstract class TimelineIndicator : MonoBehaviour {
    protected ChartEditor editor;
    [HideInInspector]
    public TimelineHandler handle;
    public int index = int.MaxValue;

    protected Vector2 previousScreenSize = Vector2.zero;

    protected virtual void Awake()
    {
        editor = ChartEditor.Instance;

        previousScreenSize.x = Screen.width;
        previousScreenSize.y = Screen.height;
    }

    protected Vector3 GetLocalPos(uint position, Song song)
    {
        float time = song.TickToTime(position, song.resolution);
        float songLength = editor.currentSongLength;
        float endTime = songLength;

        if (endTime > 0)
            return handle.HandlePosToLocal(time / endTime);
        else
            return Vector3.zero;
    }

    public abstract void ExplicitUpdate();
}
