using UnityEngine;
using System.Collections;

public abstract class TimelineIndicator : MonoBehaviour {
    protected SongObject songObject;
    protected ChartEditor editor;
    [HideInInspector]
    public TimelineHandler handle;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    protected Vector3 GetLocalPos(uint position, Song song)
    {
        float time = song.ChartPositionToTime(position, song.resolution);
        float endTime = song.length;

        if (endTime > 0)
            return handle.handlePosToLocal(time / endTime);
        else
            return Vector3.zero;
    }

    protected virtual void LateUpdate()
    {
        if (songObject != null)
            transform.localPosition = GetLocalPos(songObject.position, songObject.song);
    }
}
