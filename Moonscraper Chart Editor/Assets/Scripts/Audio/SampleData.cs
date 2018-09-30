// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using NAudio;
using NAudio.Wave;

public class SampleData {
    bool stop = false;

    float[] _data;

    public float[] data
    {
        get
        {
            return _data;
        }
    }

    Thread loadThread;
    string filepath;
    AudioStream audioStream;

    public int channels;
    public float length;

    public SampleData(string filepath)
    {
        if (filepath != string.Empty)
        {
            this.filepath = Path.GetFullPath(filepath);
        }
        else
            this.filepath = filepath;

        _data = new float[0];
        loadThread = new Thread(new ThreadStart(loadData));

        if (this.filepath != string.Empty)
        {
            audioStream = AudioManager.LoadStream(filepath);

            length = audioStream.ChannelLengthInSeconds();
            channels = 2;
        }
    }

    public bool IsLoading
    {
        get
        {
            return (loadThread.ThreadState == ThreadState.Running);
        }
    }

    public void Dispose()
    {
        stop = true;
        _data = new float[0];
        if (audioStream != null)
            audioStream.Dispose();
    }

    public void ReadAudioFile()
    {     
        _data = new float[0];

        loadThread.Start(); 
    }

    void loadData()
    {
        if (filepath != string.Empty && File.Exists(filepath))
        {
            long trackLengthInBytes = audioStream.ChannelLengthInBytes();
            const float FRAME_TIME = 0.002f;
            long frameLengthInBytes = audioStream.ChannelSecondsToBytes(FRAME_TIME);
            int NumFrames = (int)System.Math.Round(1f * trackLengthInBytes / frameLengthInBytes);
            
            _data = new float[NumFrames * 2];

            float[] levels = new float[2];
            for (int i = 0; i < _data.Length && !stop; i += 2)
            {
                audioStream.GetChannelLevels(ref levels, FRAME_TIME);
                float average = (levels[0] + levels[1]) / 2.0f;
                _data[i] = -average;
                _data[i + 1] = average;
            }

            if (!stop)
            {
                Debug.Log("Sample length: " + _data.Length);
            }
            else
            {
                _data = new float[0];
            }
        }
    }
}
