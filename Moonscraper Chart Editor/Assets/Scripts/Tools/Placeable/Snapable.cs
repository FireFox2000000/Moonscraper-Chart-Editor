// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

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

        transform.position = new Vector3(transform.position.x, editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos), transform.position.z);
    }

    protected virtual void Controls()
    {
    }

    protected void UpdateSnappedPos()
    {
        UpdateSnappedPos(GameSettings.step);
    }

    protected void UpdateSnappedPos(int step)
    {
        if (GameSettings.keysModeEnabled && Toolpane.currentTool != Toolpane.Tools.Cursor)
            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.visibleStrikeline.position.y, step);
        // Read in mouse world position
        else if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            Vector2 mousePos = (Vector2)Mouse.world2DPosition;
            float ypos = mousePos.y;

            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedChartPosition(ypos, step);
        }
        else
        {
            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.mouseYMaxLimit.position.y, step);
        }
    }

    protected void LateUpdate()
    {
        if (objectRen)
        {
            objectRen.sortingOrder = 5;
        }

        if (!Globals.IsTyping)
            Controls();
    }

    public static uint ChartPositionToSnappedChartPosition(uint chartPosition, int step, float resolution)
    {
        // Snap position based on step
        float factor = Globals.FULL_STEP / (float)step * resolution / Globals.STANDARD_BEAT_RESOLUTION;
        float divisor = chartPosition / factor;
        float lowerBound = (int)divisor * factor;
        float remainder = divisor - (int)divisor;

        if (remainder > 0.5f)
            chartPosition = (uint)Mathf.Round(lowerBound + factor);
        else
            chartPosition = (uint)Mathf.Round(lowerBound);

        return chartPosition;
    }

    public static uint ChartIncrementStep(uint chartPosition, int step, float resolution)
    {
        uint currentSnap = ChartPositionToSnappedChartPosition(chartPosition, step, resolution);

        if (currentSnap <= chartPosition)
        {
            currentSnap = ChartPositionToSnappedChartPosition(chartPosition + (uint)(Globals.FULL_STEP / (float)step * resolution / Globals.STANDARD_BEAT_RESOLUTION), step, resolution);
        }

        return currentSnap;
    }

    public static uint ChartDecrementStep(uint chartPosition, int step, float resolution)
    {
        uint currentSnap = ChartPositionToSnappedChartPosition(chartPosition, step, resolution);

        if (currentSnap >= chartPosition)
        {
            if ((uint)(Globals.FULL_STEP / (float)step * resolution / Globals.STANDARD_BEAT_RESOLUTION) >= chartPosition)
                currentSnap = 0;
            else
                currentSnap = ChartPositionToSnappedChartPosition(chartPosition - (uint)(Globals.FULL_STEP / (float)step * resolution / Globals.STANDARD_BEAT_RESOLUTION), step, resolution);
        }

        return currentSnap;
    }
}
