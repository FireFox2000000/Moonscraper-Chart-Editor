// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;

public abstract class Snapable : MonoBehaviour {
    protected ChartEditor editor;
    
    protected uint objectSnappedChartPos = 0;
    protected Renderer objectRen;

    protected virtual void Awake()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
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
        UpdateSnappedPos(GameSettings.step);
    }

    public static uint GetSnappedPos(ChartEditor editor, int step)
    {
        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            Vector2 mousePos = (Vector2)Mouse.world2DPosition;
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
        if (GameSettings.keysModeEnabled && Toolpane.currentTool != Toolpane.Tools.Cursor)
        {
            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedTick(editor.visibleStrikeline.position.y, step);
        }
        else
        {
            objectSnappedChartPos = GetSnappedPos(editor, step);
        }
    }

    protected virtual void LateUpdate()
    {
        if (objectRen)
        {
            objectRen.sortingOrder = 5;
        }
    }

    // Tuples aren't the prettiest thing in the world, but I need to return two objects, so eh
    /// <summary>
    /// Find and return the closest time signatures that are before and after the given tick in the given song.
    /// </summary>
    /// <param name="tick">The tick around which we want to find the time signatures</param>
    /// <param name="song">The song in which the time signatures are fetched</param>
    /// <returns>A tuple consisting of the closest time signature before the tick and the time signature after the tick,
    /// in that order. The one before the tick is guaranteed to exist and is therefore non-null, however the one after
    /// may not exist and thus can be null.</returns>
    private static (TimeSignature, TimeSignature) TimeSignaturesAroundTick(uint tick, Song song)
    {
        TimeSignature beforeTick = null;
        TimeSignature afterTick = null;

        // We have to go through all of the time signatures since it is probably not guaranteed
        // that the list of time signatures is sorted
        foreach (TimeSignature ts in song.timeSignatures)
        {
            // If ts is before the current tick and (after the current beforeTick candidate
            //                                      OR if there isn't any candidate)
            if (ts.tick <= tick && (beforeTick == null || beforeTick.tick < ts.tick))
            {
                beforeTick = ts;
            }
            else
            {
                // Necessarily ts.tick > tick at this point, so if this time signature is before 
                // our afterTick candidate, or if we have no afterTick candidate, replace the current
                // candidate by ts
                afterTick = ts;
            }
        }

        return (beforeTick, afterTick);
    }

    /// <summary>
    /// Snaps a tick to the given step. If a song is provided, the snapping takes time signature positions
    /// into account.
    /// </summary>
    /// <param name="tick">The tick to snap</param>
    /// <param name="step">The step to use (1/step)</param>
    /// <param name="resolution">Ticks per beat for the song</param>
    /// <param name="song">Optional: the song to apply this on. The snapping takes into account time signatures
    /// if the song is provided</param>
    /// <returns>The tick snapped to the beat with the given step</returns>
    public static uint TickToSnappedTick(uint tick, int step, float resolution, Song song = null)
    {
        if (song != null)
        {
            var (ts, nextTs) = TimeSignaturesAroundTick(tick, song);
            // ts is the time signature before the tick: guaranteed to be non-null since there is always a time
            // signature at tick 0
            // nextTs is the time signature after the tick, but can be null since it is not guaranteed that there is
            // a time signature after the position
            
            // The usual way of snapping the tick works, but we re-align it by removing the previous time signature's
            // position
            tick -= ts.tick;
            float factor2 = Song.FULL_STEP / (float) step * resolution / Song.STANDARD_BEAT_RESOLUTION;
            float divisor2 = tick / factor2;
            float lowerBound2 = (int) divisor2 * factor2;
            float remainder2 = divisor2 - (int) divisor2;

            // We re-add ts.tick in these branches
            if (remainder2 > 0.5f)
            {
                tick = (uint) Mathf.Round(lowerBound2 + factor2) + ts.tick;
                if (nextTs != null && tick > nextTs.tick)
                    // This avoids spilling over the next time signature:
                    //    if the rounding leads to us landing after the next time signature,
                    //    snap to the next time signature instead.
                    tick = nextTs.tick;
            }
            else
                tick = (uint) Mathf.Round(lowerBound2) + ts.tick;

            return tick;
        }

        // Snap position based on step
        float factor = Song.FULL_STEP / (float)step * resolution / Song.STANDARD_BEAT_RESOLUTION;
        float divisor = tick / factor;
        float lowerBound = (int)divisor * factor;
        float remainder = divisor - (int)divisor;

        if (remainder > 0.5f)
            tick = (uint)Mathf.Round(lowerBound + factor);
        else
            tick = (uint)Mathf.Round(lowerBound);

        return tick;
    }

    public static uint ChartIncrementStep(uint tick, int step, float resolution, Song song)
    {
        uint currentSnap = TickToSnappedTick(tick, step, resolution, song);

        if (currentSnap <= tick)
        {
            currentSnap = TickToSnappedTick(tick + (uint)(Song.FULL_STEP / (float)step * resolution / Song.STANDARD_BEAT_RESOLUTION), step, resolution, song);
        }

        return currentSnap;
    }

    public static uint ChartDecrementStep(uint tick, int step, float resolution, Song song)
    {
        uint currentSnap = TickToSnappedTick(tick, step, resolution, song);

        if (currentSnap >= tick)
        {
            if ((uint)(Song.FULL_STEP / (float)step * resolution / Song.STANDARD_BEAT_RESOLUTION) >= tick)
                currentSnap = 0;
            else
                currentSnap =
                    TickToSnappedTick(tick - (uint) (Song.FULL_STEP / (float) step * resolution / Song.STANDARD_BEAT_RESOLUTION), step, resolution, song);
        }

        return currentSnap;
    }
}