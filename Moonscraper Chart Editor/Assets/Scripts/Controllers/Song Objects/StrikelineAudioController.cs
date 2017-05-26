//#define BASS_AUDIO

using UnityEngine;
using System.Collections;
using Un4seen.Bass;

[RequireComponent(typeof(AudioSource))]
public class StrikelineAudioController : MonoBehaviour {

    public AudioClip clap;
    static AudioClip _clap;
    static AudioSource source; 

    static float lastClapPos = -1;
    public static float startYPoint = -1;
    Vector3 initLocalPos;

    static byte[] clapBytes;
    static int sample;

    void Start()
    {
        source = GetComponent<AudioSource>();
        _clap = clap;
#if BASS_AUDIO
        clapBytes = clap.
        sample = Bass.BASS_SampleLoad(clapBytes, 0, 0, 5, BASSFlag.BASS_DEFAULT);
#endif
        initLocalPos = transform.localPosition;  
    }
    
    void Update()
    {
        Vector3 pos = initLocalPos;
        pos.y += 0.02f * Globals.hyperspeed / Globals.gameSpeed; // Song.TimeToWorldYPosition((float)Globals.audioCalibrationMS / 1000.0f);
        transform.localPosition = pos;

        if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            lastClapPos = -1;
    }

    public static void Clap(float worldYPos)
    {
        if (worldYPos > lastClapPos && worldYPos >= startYPoint)
        {
            source.PlayOneShot(_clap);
#if BASS_AUDIO
            int channel = Bass.BASS_SampleGetChannel(sample, false); // get a sample channel
            if (channel != 0)
                Bass.BASS_ChannelPlay(channel, false); // play it
#endif
        }
        lastClapPos = worldYPos;
    }
#if BASS_AUDIO
    ~StrikelineAudioController()
    {
        Bass.BASS_SampleFree(sample);
    }
#endif
}
