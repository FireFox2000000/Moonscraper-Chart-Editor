// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

public abstract class Snapable : MonoBehaviour {
    protected ChartEditor editor;
    
    protected uint objectSnappedChartPos = 0;
    protected Renderer objectRen;

    protected virtual void Awake()
    {
        editor = ChartEditor.Instance;
        objectRen = GetComponent<Renderer>();
    }
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        UpdateSnappedPos();

        transform.position = new Vector3(transform.position.x, editor.currentSong.TickToWorldYPosition(objectSnappedChartPos), transform.position.z);

        if (!Services.IsTyping)
            Controls();
    }

    protected virtual void Controls()
    {
    }

    protected void UpdateSnappedPos()
    {
        UpdateSnappedPos(Globals.gameSettings.step);
    }

    public static uint GetSnappedPos(ChartEditor editor, int step)
    {
        if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            Vector2 mousePos = (Vector2)editor.services.mouseMonitorSystem.world2DPosition;
            float ypos = mousePos.y;
            return editor.currentSong.WorldPositionToSnappedTick(ypos, step);
        }
        else
        {
            return editor.currentSong.WorldPositionToSnappedTick(editor.mouseYMaxLimit.position.y, step);
        }
    }

    protected void UpdateSnappedPos(int step)
    {
        if (Globals.gameSettings.keysModeEnabled && editor.toolManager.currentToolId != EditorObjectToolManager.ToolID.Cursor)
        {
            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedTick(editor.visibleStrikeline.position.y, step);
        }
        else
        {
            objectSnappedChartPos = GetSnappedPos(editor, step);
        }

        // Cap to within the range of the song
        float maxTime = editor.currentSongLength;
        uint maxTick = editor.currentSong.TimeToTick(maxTime, editor.currentSong.resolution);
        objectSnappedChartPos = (uint)Mathf.Min(maxTick, objectSnappedChartPos);
    }

    protected virtual void LateUpdate()
    {
        if (objectRen)
        {
            objectRen.sortingOrder = 5;
        }
    }

    public static uint TickToSnappedTick(uint tick, int step, Song song)
    {
        float resolution = song.resolution;

        var timeSignatures = song.timeSignatures;
        int tsIndex = SongObjectHelper.FindClosestPositionRoundedDown(tick, timeSignatures);
        TimeSignature ts = timeSignatures[tsIndex];
        uint? endRange = tsIndex < (timeSignatures.Count - 1) ? (uint?)timeSignatures[tsIndex + 1].tick : null;

        TimeSignature.MeasureInfo measureInfo = ts.GetMeasureInfo();
        TimeSignature.BeatInfo measureLineInfo = measureInfo.measureLine;
        TimeSignature.BeatInfo beatLineInfo = measureInfo.beatLine;
        
        uint tickOffsetFromTs = tick - ts.tick;
        int measuresFromTsToSnap = (int)((float)tickOffsetFromTs / measureLineInfo.tickGap);
        uint lastMeasureTick = ts.tick + (uint)(measuresFromTsToSnap * measureLineInfo.tickGap);

        float realBeatStep = step / 4.0f;
        float tickGap = beatLineInfo.tickGap / realBeatStep * ts.denominator / 4.0f;
        uint tickOffsetFromLastMeasure = tick - lastMeasureTick;
        int beatsFromLastMeasureToSnap = Mathf.RoundToInt((float)tickOffsetFromLastMeasure / tickGap);

        uint snappedTick = lastMeasureTick + (uint)(beatsFromLastMeasureToSnap * tickGap + 0.5f);
        if (endRange.HasValue)
        {
            return snappedTick < endRange.Value ? snappedTick : endRange.Value;
        }
        else
        {
            return snappedTick;
        }

        // Old algorithm
        // Snap position based on step
        //float factor = SongConfig.FULL_STEP / (float)step * resolution / SongConfig.STANDARD_BEAT_RESOLUTION;
        //float divisor = tick / factor;
        //float lowerBound = (int)divisor * factor;
        //float remainder = divisor - (int)divisor;
        //
        //if (remainder > 0.5f)
        //    tick = (uint)Mathf.Round(lowerBound + factor);
        //else
        //    tick = (uint)Mathf.Round(lowerBound);
        //
        //return tick;
    }

    public static uint ChartIncrementStep(uint tick, int step, Song song)
    {
        float resolution = song.resolution;
        uint currentSnap = TickToSnappedTick(tick, step, song);

        if (currentSnap <= tick)
        {
            currentSnap = TickToSnappedTick(tick + (uint)(SongConfig.FULL_STEP / (float)step * resolution / SongConfig.STANDARD_BEAT_RESOLUTION), step, song);
        }

        return currentSnap;
    }

    public static uint ChartDecrementStep(uint tick, int step, Song song)
    {
        float resolution = song.resolution;
        uint currentSnap = TickToSnappedTick(tick, step, song);

        if (currentSnap >= tick)
        {
            if ((uint)(SongConfig.FULL_STEP / (float)step * resolution / SongConfig.STANDARD_BEAT_RESOLUTION) >= tick)
                currentSnap = 0;
            else
                currentSnap = TickToSnappedTick(tick - (uint)(SongConfig.FULL_STEP / (float)step * resolution / SongConfig.STANDARD_BEAT_RESOLUTION), step, song);
        }

        return currentSnap;
    }
}
