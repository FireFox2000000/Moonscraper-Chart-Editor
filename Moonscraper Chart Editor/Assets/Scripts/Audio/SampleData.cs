// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define BASS_AUDIO

using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using NAudio;
using NAudio.Wave;
using Un4seen.Bass;

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
    int handle = 0;

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
#if BASS_AUDIO
            handle = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE);

            length = (float)Bass.BASS_ChannelBytes2Seconds(handle, Bass.BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES));
            channels = 2;
#else
            switch (Path.GetExtension(filepath))
            {
                case (".ogg"):
                    NVorbis.VorbisReader vorbis = new NVorbis.VorbisReader(filepath);
                    channels = vorbis.Channels;
                    length = (float)vorbis.TotalTime.TotalSeconds;
                    break;
                case (".wav"):
                    WaveFileReader wav = new WaveFileReader(filepath);
                    channels = wav.WaveFormat.Channels;
                    length = (float)wav.TotalTime.TotalSeconds;
                    break;
                case (".mp3"):
                    Mp3FileReader mp3 = new Mp3FileReader(filepath);
                    channels = mp3.WaveFormat.Channels;
                    length = (float)mp3.TotalTime.TotalSeconds;
                    break;
                default:
                    break;
            }
#endif
        }
    }

    ~SampleData ()
    {
        if (handle != 0)
        {
            Bass.BASS_StreamFree(handle);
            Debug.Log("Sample handle freed");
        }
    }

    public bool IsLoading
    {
        get
        {
            return (loadThread.ThreadState == ThreadState.Running);
        }
    }

    public void Free()
    {
        stop = true;
        _data = new float[0];
        if (handle != 0)
        {
            if (Bass.BASS_StreamFree(handle))
            {
                handle = 0;
                Debug.Log("Sample handle freed");
            }
            else
                Debug.LogError("Error while freeing sample data handle: " + Bass.BASS_ErrorGetCode());
        }
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
#if BASS_AUDIO
            long trackLengthInBytes = Bass.BASS_ChannelGetLength(handle);
            const float FRAME_TIME = 0.002f;
            long frameLengthInBytes = Bass.BASS_ChannelSeconds2Bytes(handle, FRAME_TIME);
            int NumFrames = (int)System.Math.Round(1f * trackLengthInBytes / frameLengthInBytes);
            
            _data = new float[NumFrames * 2];

            float[] levels = new float[2];
            for (int i = 0; i < _data.Length && !stop; i += 2)
            {
                Bass.BASS_ChannelGetLevel(handle, levels, FRAME_TIME, BASSLevel.BASS_LEVEL_STEREO);
                float average = (levels[0] + levels[1]) / 2.0f;
                _data[i] = -average;
                _data[i + 1] = average;
            }

#else
            byte[] bytes = File.ReadAllBytes(filepath);
            float[] sampleData = new float[0]; 
            switch (Path.GetExtension(filepath))
            {
                case (".ogg"):   
                    NVorbis.VorbisReader vorbis = new NVorbis.VorbisReader(filepath);
                    vorbis.ClipSamples = false;

                    _data = new float[vorbis.TotalSamples * vorbis.Channels];
                    
                    //vorbis.ReadSamples(_data, 0, _data.Length);
                    
                    int count = 0;
                    while ((count += vorbis.ReadSamples(_data, count, 16000)) > 0 && !stop)
                    {
                    }
                    
                    break;
                case (".wav"):
                    WAV wav = new WAV(bytes);
                    sampleData = NAudioPlayer.InterleaveChannels(wav);
                    break;
                case (".mp3"):
                    NAudioPlayer.WAVFromMp3Data(bytes, out sampleData);
                    break;
                default:
                    return;
            }
#endif
            if (!stop)
            {
#if !BASS_AUDIO
                _data = sampleData;
#endif

                Debug.Log("Sample length: " + _data.Length);
            }
            else
            {
                _data = new float[0];
            }
        }
    }
}
