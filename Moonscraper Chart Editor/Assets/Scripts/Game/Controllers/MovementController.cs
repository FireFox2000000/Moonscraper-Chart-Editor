// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public abstract class MovementController : MonoBehaviour {
    const float DESYNCLENIENCE = .05f / 1000.0f;

    public static bool cancel = false;
    public ChartEditor editor;
    protected Globals globals;

    public Vector3 initPos { get; protected set; }

    protected bool focused = true;
    public static uint? explicitChartPos = null;

    protected float lastUpdatedRealTime = 0;
    protected float guiEventScrollDelta = 0;

    Transform selfTransform;

    protected float c_mouseScrollSensitivity = 0.66f;      // May miss snap gaps if placed too high
    protected float c_guiEventScrollSensitivity = 0.2f;

    // Jump to a chart position
    public abstract void SetPosition(uint tick);

    public void SetTime(float time)
    {
        //if (ChartEditor.Instance.currentState == ChartEditor.State.Editor)
        {
            Vector3 pos = initPos;
            pos.y += ChartEditor.TimeToWorldYPosition(time);
            transform.position = pos;
        }
    }

    protected void Start()
    {
        initPos = transform.position;
        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();
        selfTransform = transform;
    }

    public void PlayingMovement()
    {   
        float speed = Globals.gameSettings.hyperspeed;
        Vector3 pos = transform.position;
        float deltaTime = Time.deltaTime;
        float positionOffset = initPos.y;

        {
            float timeBeforeMovement = ChartEditor.WorldYPositionToTime(pos.y - positionOffset);
            float timeAfterMovement = timeBeforeMovement + deltaTime * Globals.gameSettings.gameSpeed;

            // Make sure we're staying in sync with the audio
            {
                SongAudioManager songAudioManager = editor.currentSongAudio;

                AudioStream stream = null;

                for (int i = 0; i < EnumX<Song.AudioInstrument>.Count; ++i)
                {
                    Song.AudioInstrument audio = (Song.AudioInstrument)i;
                    if (AudioManager.StreamIsValid(songAudioManager.GetAudioStream(audio)))
                    {
                        stream = songAudioManager.GetAudioStream(audio);
                        break;
                    }
                }

                if (AudioManager.StreamIsValid(stream) && stream.IsPlaying())
                {
                    float audioTimePosition = stream.CurrentPositionSeconds - editor.services.totalSongAudioOffset;
                    float desyncAmount = audioTimePosition - timeAfterMovement;

                    if (Mathf.Abs(desyncAmount) > DESYNCLENIENCE * Globals.gameSettings.gameSpeed)
                        timeAfterMovement += desyncAmount;
                }
            }

            float maxChangeInTimeAllowed = Application.targetFrameRate > 0 ? 2.0f / Application.targetFrameRate : 1.0f / 120.0f;

            float totalChangeInTime = timeAfterMovement - timeBeforeMovement;

            float newTimePosition = ChartEditor.TimeToWorldYPosition(timeBeforeMovement + totalChangeInTime);
            pos.y = newTimePosition + positionOffset;
        }

        selfTransform.position = pos;
        explicitChartPos = null;

        lastUpdatedRealTime = Time.time;
    }

    void OnGUI()
    {
        guiEventScrollDelta = 0;

        if (focused && UnityEngine.Event.current.type == EventType.ScrollWheel)
        {
            guiEventScrollDelta = -UnityEngine.Event.current.delta.y;
        }
    }
}
