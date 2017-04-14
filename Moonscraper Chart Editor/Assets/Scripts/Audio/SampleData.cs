using System.Collections;
using UnityEngine;
using System.Threading;
using System.IO;
using NAudio;
using NAudio.Wave;

public class SampleData {
    bool stop = false;

    float[] _data;
    float _clip = 0;
    public float clip
    {
        get
        {
            return _clip;
        }
    }

    public float[] data
    {
        get
        {
            return _data;
        }
    }

    Thread loadThread;
    string filepath;
    public float samplerate = 0;
    int sampleCount = 0;

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
            switch (Path.GetExtension(filepath))
            {
                case (".ogg"):
                    NVorbis.VorbisReader vorbis = new NVorbis.VorbisReader(filepath);
                    samplerate = vorbis.SampleRate;
                    sampleCount = (int)vorbis.TotalSamples;
                    break;
                case (".wav"):
                    WaveFileReader wav = new WaveFileReader(filepath);
                    samplerate = wav.WaveFormat.SampleRate;
                    sampleCount = (int)wav.SampleCount;
                    break;
                case (".mp3"):
                    Mp3FileReader mp3 = new Mp3FileReader(filepath);
                    samplerate = mp3.WaveFormat.SampleRate;
                    sampleCount = 0;
                    break;
                default:
                    break;
            }
        }
    }

    public bool IsLoading
    {
        get
        {
            return (loadThread.ThreadState == ThreadState.Running);
        }
    }

    public void Stop()
    {
        stop = true;

        //if (loadThread.IsAlive)
            //loadThread.Abort();
    }

    public void SetData (float[] data)
    {
        _data = data;
    }

    public void ReadAudioFile()
    {
        //if (loadThread.IsAlive)
          //  loadThread.Abort();       
        _data = new float[0];
        _clip = 0;

        loadThread.Start(); 
    }

    /// <summary>Request the samples within a specific time frame
    /// </summary>
    public float[] ReadSampleSegment(float startTime, float endTime)
    {
        float maxLength;
        float channels;

        NVorbis.VorbisReader vorbis = null;
        WaveFileReader wav = null;
        Mp3FileReader mp3 = null;

        switch (Path.GetExtension(filepath))
        {
            case (".ogg"):
                vorbis = new NVorbis.VorbisReader(filepath);              
                maxLength = (float)vorbis.TotalTime.TotalSeconds;
                channels = vorbis.Channels;
                break;
            case (".wav"):
                wav = new WaveFileReader(filepath);
                maxLength = (float)wav.TotalTime.TotalSeconds;
                channels = wav.WaveFormat.Channels;
                break;
            case (".mp3"):
                mp3 = new Mp3FileReader(filepath);
                maxLength = (float)mp3.TotalTime.TotalSeconds;
                channels = mp3.WaveFormat.Channels;
                break;
            default:
                maxLength = 0;
                channels = 2;
                break;
        }

        int startPoint = timeToArrayPos(startTime, 1, maxLength, channels);
        int endPoint = timeToArrayPos(endTime, 1, maxLength, channels);

        float[] buffer;
        if (endPoint - startPoint > 0)
            buffer = new float[endPoint - startPoint];
        else
        {
            return new float[0];
        }

        switch (Path.GetExtension(filepath))
        {
            case (".ogg"):
                vorbis.DecodedPosition = startPoint;
                vorbis.ReadSamples(buffer, 0, buffer.Length);
                break;
            case (".wav"):
                wav.Seek(startPoint, new SeekOrigin());
                wav.ToSampleProvider().Read(buffer, 0, buffer.Length);
                break;
            case (".mp3"):
                mp3.Seek(startPoint, new SeekOrigin());
                mp3.ToSampleProvider().Read(buffer, 0, buffer.Length);
                break;
            default:
                break;
        }

        return buffer;
    }

    int timeToArrayPos(float time, int iteration, float maxLength, float channels)
    {
        if (time < 0)
            return 0;
        else if (time >= maxLength)
            return sampleCount - 1;

        // Need to floor it so it lines up with the first channel
        int arrayPoint = (int)(((time / maxLength * sampleCount) / (channels * iteration)) * channels * iteration);

        if (arrayPoint >= sampleCount)
            arrayPoint = sampleCount - 1;

        return arrayPoint;
    }

    void loadData()
    {   
        if (filepath != string.Empty && File.Exists(filepath))
        {
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
                    //NAudio.Wave.Mp3FileReader mp3 = new NAudio.Wave.Mp3FileReader(filepath);
                    //mp3.ToSampleProvider().Read(new float[16000], 3, 16000);

                    break;
                default:
                    return;
            }
            
            if (!stop)
            {/*
                const int iteration = 20;
                var newData = new System.Collections.Generic.List<float>();

                for (int i = 0; i < sampleData.Length; i += (int)(channels * iteration))
                {
                    if (stop)
                        break;

                    float sampleAverage = 0;

                    for (int j = 0; j < channels; ++j)
                    {
                        sampleAverage += data[i + j];
                    }

                    sampleAverage /= channels;

                    newData.Add(sampleAverage);
                    //points.Add(new Vector3(sampleAverage * scaling, Song.TimeToWorldYPosition(i * sampleRate + fullOffset), 0));
                }
                _data = newData.ToArray();
                */
                _data = sampleData;


                Debug.Log("Sample length: " + _data.Length);

                foreach (float sample in _data)
                {
                    if (Mathf.Abs(sample) > _clip)
                        _clip = Mathf.Abs(sample);
                }

                Debug.Log("Clip: " + clip);
            }
            else
            {
                _data = new float[0];
            }
        }
    }
}
