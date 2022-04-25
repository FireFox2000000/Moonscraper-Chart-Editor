// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(LineRenderer))]
public class WaveformDraw : MonoBehaviour {
    ChartEditor editor;
    LineRenderer lineRen;

    SampleData currentSample = null;

    public Dropdown waveformSelect;
    public Text loadingText;

    int chartViewWaveformSelectionIndex = 0;
    int songViewWaveformSelectionIndex = 1;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.Instance;

        lineRen = GetComponent<LineRenderer>();
        lineRen.sortingLayerName = "Highway";

        editor.events.viewModeSwitchEvent.Register(OnViewModeSwitch);
    }

    void OnViewModeSwitch(in Globals.ViewMode viewMode)
    {
        int newIndex = 0;

        if (viewMode == Globals.ViewMode.Chart)
            newIndex = chartViewWaveformSelectionIndex;
        else if (viewMode == Globals.ViewMode.Song)
            newIndex = songViewWaveformSelectionIndex;

        waveformSelect.value = newIndex;
    }

    // Update is called once per frame
    void Update () {
        currentSample = null;
        for (int audioIndex = 0; audioIndex < EnumX<Song.AudioInstrument>.Count; ++audioIndex)
        {
            if (waveformSelect.value == (audioIndex + 1))
            {
                Song.AudioInstrument audioInstrument = (Song.AudioInstrument)audioIndex;
                currentSample = editor.currentSongAudio.GetSampleData(audioInstrument);
            }
        }

        if (Globals.viewMode == Globals.ViewMode.Chart)
            chartViewWaveformSelectionIndex = waveformSelect.value;
        else if (Globals.viewMode == Globals.ViewMode.Song)
            songViewWaveformSelectionIndex = waveformSelect.value;

        bool displayWaveform = waveformSelect.value > 0 && currentSample != null;

        loadingText.gameObject.SetActive(displayWaveform && currentSample.IsLoading);

        // Choose whether to display the waveform or not
        if (displayWaveform && currentSample.dataLength > 0)
        {
            UpdateWaveformPointsFullData();

            // Then activate
            lineRen.enabled = true;
        }
        else
        {
            lineRen.positionCount = 0;
            lineRen.enabled = false;
        }
    }

    Vector3[] points = new Vector3[0];
    void UpdateWaveformPointsFullData()
    {
        if (currentSample.dataLength <= 0 || currentSample == null)
        {
            return;
        }

        float sampleRate = currentSample.length / currentSample.dataLength;// currentClip.samples / currentClip.length;
        float scaling = 1;
        const int iteration = 1;// 20;
        int channels = 1;// currentSample.channels;
        float fullOffset = -editor.currentSong.offset;

        // Determine what points of data to draw
        int startPos = TimeToArrayPos(ChartEditor.WorldYPositionToTime(editor.camYMin.position.y) - fullOffset, iteration, channels, currentSample.length);
        int endPos = TimeToArrayPos(ChartEditor.WorldYPositionToTime(editor.camYMax.position.y) - fullOffset, iteration, channels, currentSample.length);

        int pointLength = endPos - startPos;
        if (pointLength > points.Length)
            points = new Vector3[pointLength];
#if false
        if (currentSample.clip > 0)
            scaling = (MAX_SCALE / currentSample.clip);
#endif

        // Turn data into world-position points to feed the line renderer
        Vector3 point = Vector3.zero;
        float hs = Globals.gameSettings.hyperspeed / Globals.gameSettings.gameSpeed;
        float y = 0;

        for (int i = startPos; i < endPos; ++i)
        {
            point.x = currentSample.At(i) * scaling;

            // Manual inlining of Song.TimeToWorldYPosition
            float time = i * sampleRate + fullOffset;
            point.y = time * hs;
            y = point.y;
            points[i - startPos] = point;
        }

        // Place all the unused points at the end position to prevent weird lines being drawn from the rest of them
        for (int i = pointLength; i < points.Length; ++i)
        {
            points[i].x = 0;
            points[i].y = y;
        }

        lineRen.positionCount = points.Length;
        lineRen.SetPositions(points);
    }

    int TimeToArrayPos(float time, int iteration, int channels, float totalAudioLength)
    {
        int sampleDataLength = currentSample.dataLength;
        if (time < 0)
            return 0;
        else if (time >= totalAudioLength)
            return sampleDataLength - 1;

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)((time / totalAudioLength * sampleDataLength) / (channels * iteration)) * channels * iteration;

        if (arrayPoint >= sampleDataLength)
            arrayPoint = sampleDataLength - 1;

        return arrayPoint;
    }
}
