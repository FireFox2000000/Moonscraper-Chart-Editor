using System.Collections;
using UnityEngine;
using System.Threading;
using System.IO;
using NAudio;
using NAudio.Wave;

public class SampleData {

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
            if (IsLoading)
                return new float[0];
            else
                return _data;
        }
    }

    Thread loadThread;

    string filepath;

    public SampleData()
    {
        _data = new float[0];
        loadThread = new Thread(new ThreadStart(loadData));
    }

    public bool IsLoading
    {
        get
        {
            return (loadThread.ThreadState == ThreadState.Running);
        }
    }

    public void ReadAudioFile(string filepath)
    {
        if (loadThread.IsAlive)
            loadThread.Abort();

        this.filepath = filepath;
        _data = new float[0];
        _clip = 0;

        loadThread.Start();
    }

    void loadData()
    {
        if (File.Exists(filepath))
        {
            byte[] bytes = File.ReadAllBytes(filepath);

            switch (Path.GetExtension(filepath))
            {
                case (".ogg"):
                    
                    NVorbis.VorbisReader vorbis = new NVorbis.VorbisReader(filepath);
                    vorbis.ClipSamples = false;
                    _data = new float[vorbis.TotalSamples * vorbis.Channels];
                    vorbis.ReadSamples(_data, 0, _data.Length);
                    /*int count;
                    while ((count = vorbis.ReadSamples(_data, 0, _data.Length)) > 0)
                    {
                        Debug.Log(count);
                    }*/
                    break;
                case (".wav"):
                    WAV wav = new WAV(bytes);
                    _data = NAudioPlayer.InterleaveChannels(wav);
                    break;
                case (".mp3"):
                    NAudioPlayer.WAVFromMp3Data(bytes, out _data);
                    break;
                default:
                    return;
            }
            Debug.Log("Sample length: " + _data.Length);

            foreach(float sample in _data)
            {
                if (Mathf.Abs(sample) > _clip)
                    _clip = Mathf.Abs(sample);
            }

            Debug.Log("Clip: " + clip);
        }
    }
}
