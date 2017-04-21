using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class WaveformDraw : MonoBehaviour {
    ChartEditor editor;
    LineRenderer lineRen;

    SampleData currentSample = null;

    public Dropdown waveformSelect;
    public Text loadingText;

	// Use this for initialization
	void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        lineRen = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        //if (currentSample != null)
           // Debug.Log(currentSample.data.Length);
        switch (waveformSelect.value)
        {
            case (0):
                currentSample = editor.currentSong.musicSample;
                break;
            case (1):
                currentSample = editor.currentSong.guitarSample;
                break;
            case (2):
                currentSample = editor.currentSong.rhythmSample;
                break;
            default:
                currentSample = null;
                break;
        }

        waveformSelect.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song);
        loadingText.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song && currentSample.IsLoading);

        // Choose whether to display the waveform or not
        if (Globals.viewMode == Globals.ViewMode.Song && currentSample != null && currentSample.data.Length > 0)
        {
            UpdateWaveformPointsFullData();

            // Then activate
            lineRen.enabled = true;
        }
        else
        {
            lineRen.numPositions = 0;
            lineRen.enabled = false;
        }
    }

    void UpdateWaveformPointsFullData()
    {
        if (currentSample.data.Length <= 0 || currentSample == null)
        {
            return;
        }

        float sampleRate = currentSample.length / currentSample.data.Length;// currentClip.samples / currentClip.length;
        float scaling = 1;
        const int iteration = 20;
        int channels = currentSample.channels;
        float fullOffset = -editor.currentSong.offset;
        int startPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMin.position.y) - fullOffset, iteration, channels, currentSample.length);
        int endPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMax.position.y) - fullOffset, iteration, channels, currentSample.length);

        int skipFactor = channels * iteration;

        Vector3[] points = new Vector3[Mathf.CeilToInt((endPos - startPos) / (float)skipFactor)];
#if false
        if (currentSample.clip > 0)
            scaling = (MAX_SCALE / currentSample.clip);
#endif

        Vector3 point = Vector3.zero;
        for (int i = startPos; i < endPos; i += skipFactor)
        {
            float sampleAverage = 0;

            for (int j = 0; j < channels; ++j)
            {
                sampleAverage += currentSample.data[i + j];
            }

            sampleAverage /= channels;

            point.x = sampleAverage * scaling;
            point.y = Song.TimeToWorldYPosition(i * sampleRate + fullOffset);
            points[(i - startPos) / skipFactor] = point;

            //points[i - startPos] = new Vector3(sampleAverage * scaling, Song.TimeToWorldYPosition(i * sampleRate + fullOffset), 0);
        }

        lineRen.numPositions = points.Length;
        lineRen.SetPositions(points);
    }

    int timeToArrayPos(float time, int iteration, int channels, float totalAudioLength)
    {
        if (time < 0)
            return 0;
        else if (time >= totalAudioLength)
            return currentSample.data.Length - 1;

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)((time / totalAudioLength * currentSample.data.Length) / (channels * iteration)) * channels * iteration;

        if (arrayPoint >= currentSample.data.Length)
            arrayPoint = currentSample.data.Length - 1;

        return arrayPoint;
    }
}
