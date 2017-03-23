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

    public void ReadAudioFile(string filepath)
    {
        //if (loadThread.IsAlive)
          //  loadThread.Abort();

        this.filepath = filepath;
        _data = new float[0];
        _clip = 0;

        loadThread.Start(); 
        //loadData();
    }

    void loadData()
    {   /*
        if (File.Exists(filepath))
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
                    break;
                default:
                    return;
            }
            
            if (!stop)
            {
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
        }*/
    }
}
