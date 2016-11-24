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
	protected virtual void Update () {
        // Read in mouse world position
        if (Mouse.world2DPosition != null)
        {
            Vector2 mousePos = (Vector2)Mouse.world2DPosition;
            //mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float ypos = mousePos.y;

            objectSnappedChartPos = editor.currentSong.WorldPositionToSnappedChartPosition(ypos, Globals.step);

            transform.position = new Vector3(transform.position.x, editor.currentSong.ChartPositionToWorldYPosition(objectSnappedChartPos), transform.position.z);
        }
    }

    protected virtual void Controls()
    {
    }

    protected void LateUpdate()
    {
        if (objectRen)
        {
            objectRen.sortingOrder = 5;
        }

        Controls();
    }

    protected abstract void AddObject();

    public static uint ChartPositionToSnappedChartPosition(uint chartPosition, int step, float resolution)
    {
        // Snap position based on step
        int factor = (int)(Globals.FULL_STEP / step * resolution / Globals.STANDARD_BEAT_RESOLUTION);
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
