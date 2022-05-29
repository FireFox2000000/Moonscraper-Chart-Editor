// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class DebugSongSync : MonoBehaviour
{
    bool tick = true;
    float audioPosition = 0;
    float visibleAudioTime = 0;
    float dt = 0;
    float tickFrequency = 1.0f;

    System.Array audioInstrumentEnumVals = System.Enum.GetValues(typeof(Song.AudioInstrument));

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void LateUpdate()
    {
        ChartEditor editor = ChartEditor.Instance;
        if (editor.currentState == ChartEditor.State.Playing && tick)
        {
            SongAudioManager songAudioManager = editor.currentSongAudio;
            visibleAudioTime = editor.services.currentVisualAudioTime;

            AudioStream stream = null;

            foreach (Song.AudioInstrument audio in audioInstrumentEnumVals)
            {
                if (AudioManager.StreamIsValid(songAudioManager.GetAudioStream(audio)))
                {
                    stream = songAudioManager.GetAudioStream(audio);
                    break;
                }
            }

            audioPosition = AudioManager.StreamIsValid(stream) ? stream.CurrentPositionSeconds : 0;

            tick = false;
        }

        dt += Time.deltaTime;

        if (dt > tickFrequency)
        {
            dt -= tickFrequency;
            tick = true;
        }
    }

    void OnGUI()
    {
        if (ChartEditor.Instance.currentState == ChartEditor.State.Playing)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, Screen.height - 100, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float delta = audioPosition - visibleAudioTime;
            string text = string.Format("Audio/Game desync = {0:0.00}ms", delta * 1000.0f);
            GUI.Label(rect, text, style);
        }
    }
}
