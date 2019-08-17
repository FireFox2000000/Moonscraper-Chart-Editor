// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public abstract class MovementController : MonoBehaviour {
    const float DESYNCLENIENCE = .05f / 1000.0f;

    public static bool cancel = false;
    public ChartEditor editor;
    protected Globals globals;

    public Vector3 initPos { get; protected set; }
    protected float scrollDelta = 0;

    protected bool focused = true;
    public static uint? explicitChartPos = null;

    protected float lastUpdatedRealTime = 0;
    [HideInInspector]
    public float? playStartTime;
    [HideInInspector]
    public float? playStartPosition;

    Transform selfTransform;
    System.Array audioInstrumentEnumVals = System.Enum.GetValues(typeof(Song.AudioInstrument));

    // Program options
    protected float c_mouseScrollSensitivity = 0.2f;      // May miss snap gaps if placed too high

    // Jump to a chart position
    public abstract void SetPosition(uint tick);

    public static TimeSync timeSync;

    public void SetTime(float time)
    {
        if (ChartEditor.Instance.currentState == ChartEditor.State.Editor)
        {
            Vector3 pos = initPos;
            pos.y += TickFunctions.TimeToWorldYPosition(time);
            transform.position = pos;
        }
    }

    protected void Start()
    {
        timeSync = new TimeSync();
        initPos = transform.position;
        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();
        selfTransform = transform;
    }

    public void PlayingMovement()
    {   
        float speed = GameSettings.hyperspeed;
        Vector3 pos = transform.position;
        float deltaTime = Time.deltaTime;

        {
            float timeBeforeMovement = TickFunctions.WorldYPositionToTime(pos.y);
            float timeAfterMovement = timeBeforeMovement + deltaTime;

            // Make sure we're staying in sync with the audio
            {
                Song currentSong = editor.currentSong;
                float visibleAudioTime = editor.currentAudioTime;

                AudioStream stream = null;

                foreach (Song.AudioInstrument audio in audioInstrumentEnumVals)
                {
                    if (AudioManager.StreamIsValid(currentSong.GetAudioStream(audio)))
                    {
                        stream = currentSong.GetAudioStream(audio);
                        break;
                    }
                }
                if (AudioManager.StreamIsValid(stream))
                {
                    float audioTimePosition = stream.CurrentPositionInSeconds();
                    float desyncAmount = audioTimePosition - timeAfterMovement;

                    if (Mathf.Abs(desyncAmount) > DESYNCLENIENCE)
                        timeAfterMovement += desyncAmount;
                }
            }

            float maxChangeInTimeAllowed = Application.targetFrameRate > 0 ? 2.0f / Application.targetFrameRate : 1.0f / 120.0f;

            float totalChangeInTime = timeAfterMovement - timeBeforeMovement;

            float newTimePosition = TickFunctions.TimeToWorldYPosition(timeBeforeMovement + totalChangeInTime);
            pos.y = newTimePosition;
        }

        selfTransform.position = pos;
        explicitChartPos = null;

        lastUpdatedRealTime = Time.time;
    }

    void OnGUI()
    {
        if (focused)
        {
            if (UnityEngine.Event.current.type == EventType.ScrollWheel)
            {
                scrollDelta = -UnityEngine.Event.current.delta.y;
            }
            else
            {
                scrollDelta = 0;
            }
        }
        else
            scrollDelta = 0;
    }
}
