using UnityEngine;
using System.Collections;
using System;

public class StarpowerController : SongObjectController
{
    public GameObject tail;
    public StarPower starpower { get { return (StarPower)songObject; } set { songObject = value; } }

    public StarPower unmodifiedSP = null;

    new void Awake()
    {
        base.Awake();
    }

    public void Init(StarPower _starpower)
    {
        base.Init(_starpower);
        starpower = _starpower;
        starpower.controller = this;
    }

    public override void Delete(bool update = true)
    {
        starpower.chart.Remove(starpower, update);
        Destroy(gameObject);
    }

    protected override void UpdateCheck()
    {
        if (starpower != null)
        {
            uint endPosition = starpower.position + starpower.length;

            if (    (starpower.position >= editor.minPos && starpower.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (starpower.position < editor.minPos && endPosition >= editor.maxPos))
                UpdateSongObject();
            else
                gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }

    public override void UpdateSongObject()
    {
        if (starpower.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS - 3, starpower.worldYPosition, 0);
            /*
            StarPower nextSp = null;
            if (starpower.song != null && starpower.chart != null)
            {
                // Automatic capping
                foreach (StarPower sp in starpower.chart.starPower)
                {
                    if (sp.song != null && sp.position > starpower.position)
                        nextSp = sp;
                }

                if (nextSp != null)
                {
                    // Cap sustain length
                    if (nextSp.position < starpower.position)
                        starpower.length = 0;
                    else if (starpower.position + starpower.length > nextSp.position)
                        // Cap sustain
                        starpower.length = nextSp.position - starpower.position;
                }
                // else it's the only starpower or it's the last starpower 
            }*/

            UpdateTailLength();
        }
    }

    public void UpdateTailLength()
    {
        float length = starpower.song.ChartPositionToWorldYPosition(starpower.position + starpower.length) - starpower.worldYPosition;

        Vector3 scale = tail.transform.localScale;
        scale.y = length;
        tail.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        tail.transform.position = position;
    }

    public void TailDrag()
    {
        ChartEditor.editOccurred = true;
        uint snappedChartPos;

        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(starpower.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, starpower.song.resolution);           
        }
        else
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(starpower.song.WorldYPositionToChartPosition(editor.mouseYMaxLimit.position.y), Globals.step, starpower.song.resolution);
        }

        if (snappedChartPos > starpower.position)
            starpower.length = snappedChartPos - starpower.position;
        else
            starpower.length = 0;     
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Pass note data to a ghost note
            GameObject moveSP = Instantiate(editor.starpowerPrefab);
            moveSP.SetActive(true);
            moveSP.name = "Moving starpower";
            Destroy(moveSP.GetComponent<PlaceStarpower>());
            moveSP.AddComponent<MoveStarpower>().Init(starpower);
                
            editor.currentSelectedObject = starpower;
            moveSP.SetActive(true);

            Delete();
        }
        else 
        {
            dragCheck();
        }
    }

    public void dragCheck()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            if (unmodifiedSP == null)
                unmodifiedSP = (StarPower)starpower.Clone();

            TailDrag();
        }
    }

    public override void OnSelectableMouseUp()
    {
        if (unmodifiedSP != null && unmodifiedSP.length != starpower.length)
        {
            editor.actionHistory.Insert(new ActionHistory.Modify(unmodifiedSP, starpower));
        }

        unmodifiedSP = null;
    }
}
