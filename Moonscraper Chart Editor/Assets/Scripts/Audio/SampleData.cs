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
            BASS_SAMPLE info = Bass.BASS_SampleGetInfo(handle);

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
                Debug.LogError("Error while freeing sample data handle");
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
            /*
            int byteLength = (int)Bass.BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES | BASSMode.BASS_POS_OGG);
            Debug.Log(byteLength);
            Debug.Log(handle);*/

            long trackLengthInBytes = Bass.BASS_ChannelGetLength(handle);
            const float FRAME_TIME = 0.002f;
            long frameLengthInBytes = Bass.BASS_ChannelSeconds2Bytes(handle, FRAME_TIME);
            int NumFrames = (int)System.Math.Round(1f * trackLengthInBytes / frameLengthInBytes);
            
            _data = new float[NumFrames * 2];

            float[] levels = new float[2];
            for (int i = 0; i < _data.Length; i += 2)
            {
                Bass.BASS_ChannelGetLevel(handle, levels, FRAME_TIME, BASSLevel.BASS_LEVEL_STEREO);
                float average = (levels[0] + levels[1]) / 2.0f;
                _data[i] = -average;
                _data[i + 1] = average;
            }
#if true

#elif true
            float[] samples = new float[byteLength / sizeof(float)];
            int length = Bass.BASS_ChannelGetData(handle, samples, byteLength);
            if (length == -1)
            {
                Debug.LogError(Bass.BASS_ErrorGetCode());
                return;
            }
            _data = new float[(int)System.Math.Ceiling((float)length / (iteration * (float)channels) / sizeof(float))];

            int count = 0;
            for (int i = 0; i + channels < samples.Length; i += iteration * channels)
            {
                
                    if (stop)
                        break;

                    float sampleAverage = 0;

                    for (int j = 0; j < channels; ++j)
                    {
                    try
                    {
                        sampleAverage += samples[i + j];
                    }
                    catch (System.Exception e)
                    {
                        //Debug.LogError("1: " + count + ", " + e.Message);
                    }
                }

                    sampleAverage /= channels;
                try
                {
                    _data[count++] = sampleAverage;
                }
                catch (System.Exception e)
                {
                    //Debug.LogError("2: " + count + ", " + e.Message);
                }

            }

#else

            _data = new float[(int)System.Math.Ceiling((float)byteLength / (iteration * (float)channels) / sizeof(float))];
            int offset = 0;
            int count = 0;
            int startOffset = 0;
            float[] samples = new float[32768];
            while (Bass.BASS_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_PLAYING && !stop)
            {
                int length = Bass.BASS_ChannelGetData(handle, samples, samples.Length);
                Debug.Log(length);
                try
                {
                    int i;

                    for (i = startOffset; i < length; i += iteration * channels)
                    {                 
                        float sampleAverage = 0;

                        for (int j = 0; j < channels; ++j)
                        {
                            sampleAverage += samples[i + j];
                        }

                        sampleAverage /= channels;
                        
                        _data[count++] = sampleAverage;
                    }
                
                    //System.Buffer.BlockCopy(samples, 0, _data, offset, length);
                    startOffset = i - length;
                    offset += length;
                }
                catch (System.Exception e) { Debug.LogError(e.Message); }
            }
#endif
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
                        //count += vorbis.Channels * 20;
                        //vorbis.DecodedPosition += 2000;
                        //Debug.Log(loadThread.ThreadState);
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
