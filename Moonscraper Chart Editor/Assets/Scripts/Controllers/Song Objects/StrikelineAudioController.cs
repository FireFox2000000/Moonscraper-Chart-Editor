#define BASS_AUDIO

using UnityEngine;
using System.Collections;
using Un4seen.Bass;

[RequireComponent(typeof(AudioSource))]
public class StrikelineAudioController : MonoBehaviour {

    public AudioClip clap;
#if !BASS_AUDIO
    static AudioClip _clap;
    static AudioSource source; 
#endif
    static float lastClapPos = -1;
    public static float startYPoint = -1;
    Vector3 initLocalPos;

    static byte[] clapBytes;
    static int sample;

    void Start()
    {   
#if BASS_AUDIO
        clapBytes = clap.GetWavBytes();
        sample = Bass.BASS_SampleLoad(clapBytes, 0, clapBytes.Length, 15, BASSFlag.BASS_DEFAULT);
#else
        source = GetComponent<AudioSource>();
        _clap = clap;
#endif
        initLocalPos = transform.localPosition;  
    }
    
    void Update()
    {
        Vector3 pos = initLocalPos;
        pos.y += 0.02f * Globals.hyperspeed / Globals.gameSpeed;
        transform.localPosition = pos;

        if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            lastClapPos = -1;
    }

    public static void Clap(float worldYPos)
    {
        if (worldYPos > lastClapPos && worldYPos >= startYPoint)
        {

#if BASS_AUDIO
            int channel = Bass.BASS_SampleGetChannel(sample, false); // get a sample channel
            if (channel != 0)
            {
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, Globals.sfxVolume * Globals.vol_master);
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, Globals.audio_pan);
                Bass.BASS_ChannelPlay(channel, false); // play it
            }
            else
                Debug.LogError("Clap error: " + Bass.BASS_ErrorGetCode() + ", " + sample);
#else
            source.PlayOneShot(_clap);
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
