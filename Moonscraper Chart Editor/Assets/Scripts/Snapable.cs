using UnityEngine;
using System.Collections;

public class Snapable : MonoBehaviour {
    public ChartEditor editor;
	
	// Update is called once per frame
	protected void Update () {
        // Read in mouse world position
        float ypos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

        transform.position = new Vector3(transform.position.x, editor.currentSong.ChartPositionToWorldYPosition(WorldPositionToSnappedChartPosition(ypos, Globals.step)), transform.position.z);
    }

    public uint WorldPositionToSnappedChartPosition(float worldYPos, int step)
    {
        uint chartPos = editor.currentSong.WorldYPositionToChartPosition(worldYPos);

        return ChartPositionToSnappedChartPosition(chartPos, step);
    }

    public static uint ChartPositionToSnappedChartPosition(uint chartPosition, int step)
    {
        // Snap position based on step
        int factor = (int)Globals.FULL_STEP / step;
        float divisor = (float)chartPosition / (float)factor;
        uint lowerBound = (uint)((int)divisor * factor);
        float remainder = divisor - (int)divisor;

        if (remainder > 0.5f)
            chartPosition = lowerBound + (uint)factor;
        else
            chartPosition = (lowerBound);

        return chartPosition;
    }
}
