﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class SampleData {
    private readonly object dataLock = new object();
    bool stop = false;

    float[] _data;
    float _length;

    Thread loadThread;
    string filepath;
    AudioStream audioStream;

    public const int channels = 2;
    
    public float length
    {
        get
        {
            lock (dataLock)
            {
                return _length;
            }
        }
    }

    public int dataLength
    {
        get
        {
            lock (dataLock)
            {
                return _data.Length;
            }
        }
    }

    public SampleData(string filepath)
    {
        if (filepath != string.Empty)
        {
            this.filepath = Path.GetFullPath(filepath);
        }
        else
            this.filepath = filepath;

        _data = new float[0];
        loadThread = new Thread(new ThreadStart(LoadData));
        loadThread.Start();
    }

    public bool IsLoading
    {
        get
        {
            return (loadThread.ThreadState == ThreadState.Running);
        }
    }

    public float At(int index)
    {
        lock (dataLock)
        {
            return _data[index];
        }
    }

    public void Dispose()
    {
        stop = true;
        while (IsLoading) ;

        if (audioStream != null)
            audioStream.Dispose();
    }

    void LoadData()
    {
        if (!string.IsNullOrEmpty(filepath) && File.Exists(filepath))
        {
            audioStream = AudioManager.LoadStream(filepath);
            lock (dataLock)
            {
                _length = audioStream.ChannelLengthInSeconds();
            }

            long trackLengthInBytes = audioStream.ChannelLengthInBytes();
            const float FRAME_TIME = 0.002f;
            long frameLengthInBytes = audioStream.ChannelSecondsToBytes(FRAME_TIME);
            int NumFrames = (int)System.Math.Round(1f * trackLengthInBytes / frameLengthInBytes);

            lock (dataLock)
            {
                _data = new float[NumFrames * 2];
            }

            float[] levels = new float[2];
            for (int i = 0; i < _data.Length && !stop; i += 2)
            {
                lock (dataLock)
                {
                    audioStream.GetChannelLevels(ref levels, FRAME_TIME);
                    float average = (levels[0] + levels[1]) / 2.0f;
                    _data[i] = -average;
                    _data[i + 1] = average;
                }
            }

            if (!stop)
            {
                Debug.Log("Sample length: " + _data.Length);
            }
            else
            {
                lock (dataLock)
                {
                    _data = new float[0];
                }
            }
        }
    }
}
