using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class WaveformDraw : MonoBehaviour {
    ChartEditor editor;
    LineRenderer lineRen;
    float[] data = new float[0];
    AudioClip currentClip = null;

	// Use this for initialization
	void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
        lineRen = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        // Get new data
        if (Globals.viewMode == Globals.ViewMode.Song && (currentClip == null || currentClip != editor.currentSong.musicStream))
        {
            currentClip = editor.currentSong.musicStream;

            if (currentClip == null)
            {
                data = new float[0];
            }
            else
            {
                data = new float[currentClip.samples * currentClip.channels];
                currentClip.GetData(data, 0);
            }
        } 

        // Choose whether to display the waveform or not
	    if (Globals.viewMode == Globals.ViewMode.Song && editor.currentSong.musicStream != null)
        {
            UpdateWaveformPoints();

            // Then activate
            lineRen.enabled = true;          
        }
        else
            lineRen.enabled = false;
    }

    void UpdateWaveformPoints()
    {
        if (data.Length <= 0 || currentClip == null)
            return;

        float sampleRate = currentClip.length / data.Length;// currentClip.samples / currentClip.length;

        int iteration = 20;
        int startPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMin.position.y), iteration);
        int endPos = timeToArrayPos(Song.WorldYPositionToTime(editor.camYMax.position.y), iteration);

        List<Vector3> points = new List<Vector3>();
        for (int i = startPos; i < endPos; i += (int)(currentClip.channels * iteration))
        {
            float sampleAverage = 0;

            for (int j = 0; j < currentClip.channels; ++j)
            {
                sampleAverage += data[i + j];
            }

            sampleAverage /= currentClip.channels;

            points.Add(new Vector3(sampleAverage, Song.TimeToWorldYPosition(i * sampleRate), 0));
        }

        lineRen.SetVertexCount(points.Count);
        lineRen.SetPositions(points.ToArray());
    }

    int timeToArrayPos(float time, int iteration)
    {
        if (time < 0)
            return 0;
        else if (time >= currentClip.length)
            return data.Length - 1;

        // Get the point the data should start reading from
        int singleChannelLength = data.Length / currentClip.channels;
        //int arrayPoint = (int)(time / currentClip.length * data.Length);

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)((time / currentClip.length * data.Length) / (currentClip.channels * iteration)) * currentClip.channels * iteration;

        if (arrayPoint >= data.Length)
            arrayPoint = data.Length - 1;

        return arrayPoint;
    }
}
