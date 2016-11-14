using UnityEngine;
using System.Collections;
using System;

public class TimelineMovementController : MovementController
{
    public TimelineHandler timeline;

    public override void SetPosition(uint chartPosition)
    {
        if (applicationMode == ApplicationMode.Editor)
        {
            Vector3 pos = initPos;
            pos.y += editor.currentSong.ChartPositionToWorldYPosition(chartPosition);
            transform.position = pos;
        }
    }

    // Use this for initialization
    new void Start () {
        base.Start();
        timeline.handlePos = 0;
        UpdatePosBasedTimelineHandle();
    }

    Vector3 prevPos = Vector3.zero;

    // Update is called once per frame
    void FixedUpdate () {
	    if (applicationMode == ApplicationMode.Editor)
        {
            if (scrollDelta == 0)
            {
                scrollDelta = Input.mouseScrollDelta.y;
            }

            // Position changes scroll bar value
            if (scrollDelta != 0 || transform.position != prevPos)
            {
                // Mouse scroll movement
                transform.position = new Vector3(transform.position.x, transform.position.y + (scrollDelta * mouseScrollSensitivity), transform.position.z);

                if (transform.position.y < initPos.y)
                    transform.position = initPos;

                UpdateTimelineHandleBasedPos();
            }

            // Scroll bar value changes position
            else
            {
                UpdatePosBasedTimelineHandle();
            }
        }
        else if(applicationMode == ApplicationMode.Playing)
        {
            PlayingMovement();

            // Update timeline handle
            UpdateTimelineHandleBasedPos();

            if (timeline.handlePos >= 1)
                editor.Stop();
        }

        prevPos = transform.position;
    }

    void UpdateTimelineHandleBasedPos()
    {
        if (editor.currentChart != null)
        {
            float endYPos = Song.TimeToWorldYPosition(editor.currentSong.length);
            float totalDistance = endYPos - initPos.y;
            float currentDistance = transform.position.y - initPos.y;

            if (totalDistance > 0)
                timeline.handlePos = currentDistance / totalDistance;
            else
                timeline.handlePos = 0;
        }
    }

    void UpdatePosBasedTimelineHandle()
    {      
        if (editor.currentChart != null)
        {         
            float endYPos = Song.TimeToWorldYPosition(editor.currentSong.length);
            float totalDistance = endYPos - initPos.y;

            if (totalDistance > 0)
            {
                float currentDistance = timeline.handlePos * totalDistance;

                transform.position = initPos + new Vector3(0, currentDistance, 0);
            }
            else
            {
                timeline.handlePos = 0;
                transform.position = initPos;
            }
        }
    }
}
