//#define REQUESTED_SAMPLE

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class WaveformDraw : MonoBehaviour {
    ChartEditor editor;
    LineRenderer lineRen;
    float[] data = new float[0];
    SampleData currentSample = null;
    AudioClip currentAudio = null;

    SampleData selectedSample = null;
    AudioClip selectedAudio = null;

    public Dropdown waveformSelect;
    public Text loadingText;

	// Use this for initialization
	void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        lineRen = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        switch (waveformSelect.value)
        {
            case (0):
                selectedSample = editor.currentSong.musicSample;
                selectedAudio = editor.currentSong.musicStream;
                break;
            case (1):
                selectedSample = editor.currentSong.guitarSample;
                selectedAudio = editor.currentSong.guitarStream;
                break;
            case (2):
                selectedSample = editor.currentSong.rhythmSample;
                selectedAudio = editor.currentSong.rhythmStream;
                break;
            default:
                break;
        }

        waveformSelect.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song);
        loadingText.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song && selectedSample.IsLoading);

        // Get new data
        if (Globals.viewMode == Globals.ViewMode.Song && (currentSample == null || currentSample != selectedSample || data.Length == 0))
        {
            currentAudio = selectedAudio;
            currentSample = selectedSample;
#if !REQUESTED_SAMPLE
            if (currentSample == null)
            {
                data = new float[0];
            }
            else
            {
                data = currentSample.data;
            }
#endif
        }

#if REQUESTED_SAMPLE
        if (Globals.viewMode == Globals.ViewMode.Song)
        {
            if (currentSample == null)
            {
                data = new float[0];
            }
            else
            {              
                data = currentSample.ReadSampleSegment(Song.WorldYPositionToTime(editor.camYMin.position.y), Song.WorldYPositionToTime(editor.camYMax.position.y));

            }
        }
#endif

        // Choose whether to display the waveform or not
        if (Globals.viewMode == Globals.ViewMode.Song && currentSample != null && currentAudio != null && data.Length > 0)
        {
#if REQUESTED_SAMPLE
            UpdateWaveformPointRequestedData();
#else
            UpdateWaveformPointsFullData();
#endif

            // Then activate
            lineRen.enabled = true;          
        }
        else
            lineRen.enabled = false;
    }

    void UpdateWaveformPointRequestedData()
    {
        const float MAX_SCALE = 2.5f;

        if (data.Length <= 0 || currentSample == null)
        {
            return;
        }

        int iteration = 20;
        float scaling = 1;
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < data.Length; i += (int)(currentAudio.channels * iteration))
        {
            float sampleAverage = 0;

            for (int j = 0; j < currentAudio.channels; ++j)
            {
                sampleAverage += data[i + j];
            }

            sampleAverage /= currentAudio.channels;

            points.Add(new Vector3(sampleAverage * scaling, Song.TimeToWorldYPosition(i * currentSample.samplerate), 0));
        }

        lineRen.numPositions = points.Count;
        lineRen.SetPositions(points.ToArray());
    }

    void UpdateWaveformPointsFullData()
    {
        const float MAX_SCALE = 2.5f;

        if (data.Length <= 0 || currentSample == null)
        {
            return;
        }

        float sampleRate = currentAudio.length / data.Length;// currentClip.samples / currentClip.length;

        int iteration = 20;
        int startPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMin.position.y), iteration);
        int endPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMax.position.y), iteration);

        List<Vector3> points = new List<Vector3>();

        float scaling = 1;
#if false
        if (currentSample.clip > 0)
            scaling = (MAX_SCALE / currentSample.clip);
#endif
        for (int i = startPos; i < endPos; i += (int)(currentAudio.channels * iteration))
        {
            float sampleAverage = 0;

            for (int j = 0; j < currentAudio.channels; ++j)
            {
                sampleAverage += data[i + j];
            }

            sampleAverage /= currentAudio.channels;

            points.Add(new Vector3(sampleAverage * scaling, Song.TimeToWorldYPosition(i * sampleRate), 0));
        }

        lineRen.numPositions = points.Count;
        lineRen.SetPositions(points.ToArray());
    }

    int timeToArrayPos(float time, int iteration)
    {
        if (time < 0)
            return 0;
        else if (time >= currentAudio.length)
            return data.Length - 1;

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)((time / currentAudio.length * data.Length) / (currentAudio.channels * iteration)) * currentAudio.channels * iteration;

        if (arrayPoint >= data.Length)
            arrayPoint = data.Length - 1;

        return arrayPoint;
    }
}
