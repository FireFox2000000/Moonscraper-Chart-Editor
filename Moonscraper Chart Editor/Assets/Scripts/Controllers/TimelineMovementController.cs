using UnityEngine;
using System.Collections;
using System;

public class TimelineMovementController : MovementController
{
    public TimelineHandler timeline;
    public Transform strikeLine;
    public UnityEngine.UI.Text timePosition;

    const float autoscrollSpeed = 10.0f;

    public override void SetPosition(uint chartPosition)
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
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

    void Update()
    {
        // Update timer text
        if (timePosition)
        {
            if (editor.currentSong.musicStream == null)
            {
                timePosition.color = Color.red;
                timePosition.text = "No audio";               
            }
            else
            {
                timePosition.color = Color.white;
                timePosition.text = Utility.timeConvertion(Song.WorldYPositionToTime(strikeLine.position.y));
            }
        }
    }

    Vector3 prevPos = Vector3.zero;

    // Update is called once per frame
    void LateUpdate () {
        
	    if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            if (scrollDelta == 0 && focused && globals.InToolArea)
            {
                scrollDelta = Input.mouseScrollDelta.y;
            }

            // Position changes scroll bar value
            if (scrollDelta != 0 || transform.position != prevPos)
            {
                if (Input.GetKey(KeyCode.LeftAlt) && editor.currentSong.sections.Length > 0)
                {
                    // Jump to the previous or next sections
                    float position = Mathf.Round(strikeLine.position.y);

                    int i = 0;
                    while (i < editor.currentSong.sections.Length && Mathf.Round(editor.currentSong.sections[i].worldYPosition) <= position)
                    {
                        ++i;
                    }
                    
                    // Jump forward
                    if (scrollDelta > 0)
                    {
                        // Found section ahead
                        if (i < editor.currentSong.sections.Length && Mathf.Round(editor.currentSong.sections[i].worldYPosition) > position)
                            SetPosition(editor.currentSong.sections[i].position);
                        else
                            SetPosition(editor.currentSong.TimeToChartPosition(editor.currentSong.length, editor.currentSong.resolution));       // Jump to the end of the song

                    }
                    // Jump backwards
                    else
                    {
                        
                        while (i > editor.currentSong.sections.Length - 1 || (i >= 0 && Mathf.Round(editor.currentSong.sections[i].worldYPosition) >= position))
                            --i;

                        if (i >= 0)
                            SetPosition(editor.currentSong.sections[i].position);
                        else
                            SetPosition(0);
                    }

                }
                else
                    // Mouse scroll movement
                    transform.position = new Vector3(transform.position.x, transform.position.y + (scrollDelta * mouseScrollSensitivity), transform.position.z);

                if (transform.position.y < initPos.y)
                    transform.position = initPos;

                UpdateTimelineHandleBasedPos();
            }
            // else check mouse range
            else if (Toolpane.mouseDownInArea && (globals.InToolArea && (Input.GetMouseButton(0) || Input.GetMouseButton(1))))
            { 
                if (Input.mousePosition.y > Camera.main.WorldToScreenPoint(editor.mouseYMaxLimit.position).y)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + autoscrollSpeed * Time.deltaTime, transform.position.z);
                    UpdateTimelineHandleBasedPos();
                }
                else
                {
                    UpdatePosBasedTimelineHandle();
                }
            }
            // Scroll bar value changes position
            else
            {
                UpdatePosBasedTimelineHandle();
            }
        }
        else if(Globals.applicationMode == Globals.ApplicationMode.Playing)
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
            // Front cap
            if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            {
                if (transform.position.y < initPos.y)
                    transform.position = initPos;
            }

            float endYPos = Song.TimeToWorldYPosition(editor.currentSong.length);
            float totalDistance = endYPos - initPos.y - strikeLine.localPosition.y;

            if (transform.position.y + strikeLine.localPosition.y > endYPos)
            {
                transform.position = new Vector3(transform.position.x, endYPos - strikeLine.localPosition.y, transform.position.z);
            }

            float currentDistance = transform.position.y - initPos.y;

            //if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            //{
                if (totalDistance > 0)
                    timeline.handlePos = currentDistance / totalDistance;
                else
                    timeline.handlePos = 0;
            //}
        }
    }

    void UpdatePosBasedTimelineHandle()
    {
        if (editor.currentChart != null)
        {         
            float endYPos = Song.TimeToWorldYPosition(editor.currentSong.length);
            float totalDistance = endYPos - initPos.y - strikeLine.localPosition.y;

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
