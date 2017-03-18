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

            explicitChartPos = chartPosition;
        }
    }

    // Use this for initialization
    new void Start () {
        base.Start();
        timeline.handlePos = 0;
        prevScreenSize = new Vector2(Screen.width, Screen.height);
        UpdatePosBasedTimelineHandle();
    }

    const float ARROW_INIT_DELAY_TIME = 0.5f;
    const float ARROW_HOLD_MOVE_ITERATION_TIME = 0.1f;
    float arrowMoveTimer = 0;
    float lastMoveTime = 0;  

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && Globals.applicationMode == Globals.ApplicationMode.Editor)
            cancel = false;

        if (Globals.IsInDropDown)
            cancel = true;

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

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            arrowMoveTimer = 0;
        else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            arrowMoveTimer += Time.deltaTime;
        else
            arrowMoveTimer = 0;
    }

    Vector3 prevPos = Vector3.zero;
    Vector3 lastMouseDownPos = Vector3.zero;
    Vector2 prevScreenSize;
    // Update is called once per frame
    void LateUpdate () {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            lastMouseDownPos = Input.mousePosition;
        }

        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            if (scrollDelta == 0 && focused /*&& globals.InToolArea*/)
            {
                scrollDelta = Input.mouseScrollDelta.y;
            }

            if (Globals.IsInDropDown /*&& Mouse.GetUIUnderPointer<UnityEngine.UI.Dropdown>()*/)
                scrollDelta = 0;

            // Position changes scroll bar value
            if (scrollDelta != 0 || transform.position != prevPos || prevScreenSize != new Vector2(Screen.width, Screen.height))
            {
                if (Input.GetKey(KeyCode.LeftAlt) && editor.currentSong.sections.Length > 0)
                {
                    SectionJump(scrollDelta);
                }
                else
                {
                    // Mouse scroll movement
                    transform.position = new Vector3(transform.position.x, transform.position.y + (scrollDelta * mouseScrollSensitivity), transform.position.z);
                    explicitChartPos = null;
                }

                if (transform.position.y < initPos.y)
                    transform.position = initPos;

                if (prevScreenSize != new Vector2(Screen.width, Screen.height))
                    StartCoroutine(resolutionChangePosHold());

                UpdateTimelineHandleBasedPos();
            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.LeftAlt) && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)))
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                        SectionJump(1);
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                        SectionJump(-1);
                }
                else
                {
                    // Arrow key controls
                    uint currentPos;
                    if (explicitChartPos != null)
                        currentPos = (uint)explicitChartPos;
                    else
                        currentPos = editor.currentSong.WorldYPositionToChartPosition(editor.visibleStrikeline.position.y);

                    if (arrowMoveTimer == 0 || (arrowMoveTimer > ARROW_INIT_DELAY_TIME && Time.realtimeSinceStartup > lastMoveTime + ARROW_HOLD_MOVE_ITERATION_TIME))
                    {
                        uint snappedPos;
                        // Navigate to snapped pos ahead or behind
                        if (Input.GetKey(KeyCode.UpArrow))
                        {
                            snappedPos = Snapable.ChartIncrementStep(currentPos, Globals.step, editor.currentSong.resolution);
                        }
                        else
                        {
                            snappedPos = Snapable.ChartDecrementStep(currentPos, Globals.step, editor.currentSong.resolution);
                        }

                        if (editor.currentSong.ChartPositionToTime(snappedPos, editor.currentSong.resolution) <= editor.currentSong.length)
                        {
                            SetPosition(snappedPos);
                        }

                        lastMoveTime = Time.realtimeSinceStartup;
                    }
                }

                UpdateTimelineHandleBasedPos();
            }
            // else check mouse range
            else if (Toolpane.mouseDownInArea && (globals.InToolArea && (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && Input.mousePosition != lastMouseDownPos))
            { 
                if (!Toolpane.menuCancel && 
                    UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null && 
                    Input.mousePosition.y > Camera.main.WorldToScreenPoint(editor.mouseYMaxLimit.position).y)
                {
                    // Autoscroll
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
        prevScreenSize = new Vector2(Screen.width, Screen.height);
    }

    IEnumerator resolutionChangePosHold()
    {
        yield return null;

        UpdateTimelineHandleBasedPos();
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

    void SectionJump(float direction)
    {
        // Jump to the previous or next sections
        float position = Mathf.Round(strikeLine.position.y);

        int i = 0;
        while (i < editor.currentSong.sections.Length && Mathf.Round(editor.currentSong.sections[i].worldYPosition) <= position)
        {
            ++i;
        }

        // Jump forward
        if (direction > 0)
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
}
