using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Song currentSong = editor.currentSong;
            visibleAudioTime = editor.currentAudioTime;

            AudioStream stream = null;

            foreach (Song.AudioInstrument audio in audioInstrumentEnumVals)
            {
                if (AudioManager.StreamIsValid(currentSong.GetAudioStream(audio)))
                {
                    stream = currentSong.GetAudioStream(audio);
                    break;
                }
            }

            audioPosition = AudioManager.StreamIsValid(stream) ? stream.CurrentPositionInSeconds() : 0;

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
