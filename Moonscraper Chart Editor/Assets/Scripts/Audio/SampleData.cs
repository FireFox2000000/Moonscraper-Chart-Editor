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
            handle = Bass.BASS_StreamCreateFile(filepath, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
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

        loadThread.Start(); 
    }

    void loadData()
    {
        if (filepath != string.Empty && File.Exists(filepath))
        {
#if BASS_AUDIO
            float[] samples = new float[32768];
            int byteLength = (int)Bass.BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES);

            _data = new float[byteLength / 4];
            int offset = 0;

            while (Bass.BASS_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_PLAYING && !stop)
            {
                int length = Bass.BASS_ChannelGetData(handle, samples, samples.Length * 4);
                System.Buffer.BlockCopy(samples, 0, _data, offset, length);

                offset += length;
            }
#else
            byte[] bytes = File.ReadAllBytes(filepath);
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
#endif
            if (!stop)
            {
                //_data = sampleData;

                Debug.Log("Sample length: " + _data.Length);
            }
            else
            {
                _data = new float[0];
            }
        }
    }
}
