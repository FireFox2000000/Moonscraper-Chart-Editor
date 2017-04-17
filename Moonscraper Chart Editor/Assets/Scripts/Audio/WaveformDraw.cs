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
    //AudioClip currentAudio = null;

    SampleData selectedSample = null;
    //AudioClip selectedAudio = null;

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
                //selectedAudio = editor.currentSong.musicStream;
                break;
            case (1):
                selectedSample = editor.currentSong.guitarSample;
               // selectedAudio = editor.currentSong.guitarStream;
                break;
            case (2):
                selectedSample = editor.currentSong.rhythmSample;
                //selectedAudio = editor.currentSong.rhythmStream;
                break;
            default:
                break;
        }

        waveformSelect.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song);
        loadingText.gameObject.SetActive(Globals.viewMode == Globals.ViewMode.Song && selectedSample.IsLoading);

        // Get new data
        if (Globals.viewMode == Globals.ViewMode.Song && (currentSample == null || currentSample != selectedSample || data.Length == 0))
        {
            //currentAudio = selectedAudio;
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
        if (Globals.viewMode == Globals.ViewMode.Song && currentSample != null && data.Length > 0)
        {
#if REQUESTED_SAMPLE
            UpdateWaveformPointRequestedData();
#else
            UpdateWaveformPointsFullData();
            //UpdateWaveformPointsFullCompressedData();
#endif

            // Then activate
            lineRen.enabled = true;          
        }
        else
            lineRen.enabled = false;
    }
    /*
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
    
    void UpdateWaveformPointsFullCompressedData()
    {
        if (data.Length <= 0 || currentSample == null)
        {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        float fullOffset = editor.currentSong.offset - (Globals.audioCalibrationMS / 1000.0f);
        int startPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMin.position.y) - fullOffset, 1, 1);
        int endPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMax.position.y) - fullOffset, 1, 1);
        float scaling = 1;

        for (int i = startPos; i < endPos; ++i)
        {
            points.Add(new Vector3(data[i] * scaling, Song.TimeToWorldYPosition(i * data[i] + fullOffset), 0));
        }

        lineRen.numPositions = points.Count;
        lineRen.SetPositions(points.ToArray());
    }*/
    void UpdateWaveformPointsFullData()
    {
        if (data.Length <= 0 || currentSample == null)
        {
            return;
        }

        float sampleRate = currentSample.length / data.Length;// currentClip.samples / currentClip.length;
        float scaling = 1;
        const int iteration = 20;
        int channels = currentSample.channels;
        float fullOffset = -editor.currentSong.offset - (Globals.audioCalibrationMS / 1000.0f);
        int startPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMin.position.y) - fullOffset, iteration, channels, currentSample.length);
        int endPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMax.position.y) - fullOffset, iteration, channels, currentSample.length);

        int skipFactor = channels * iteration;
        Vector3[] points = new Vector3[Mathf.CeilToInt((endPos - startPos) / (float)skipFactor)];
        //List<Vector3> points = new List<Vector3>();
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
                sampleAverage += data[i + j];
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
            return data.Length - 1;

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)((time / totalAudioLength * data.Length) / (channels * iteration)) * channels * iteration;

        if (arrayPoint >= data.Length)
            arrayPoint = data.Length - 1;

        return arrayPoint;
    }
}
