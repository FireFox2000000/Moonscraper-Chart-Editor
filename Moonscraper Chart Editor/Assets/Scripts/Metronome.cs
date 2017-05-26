#define BASS_AUDIO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Un4seen.Bass;

public class Metronome : MonoBehaviour {
    ChartEditor editor;
    AudioSource clapSource;
    Vector3 initLocalPos;

    public AudioClip clap;
    static byte[] clapBytes;
    static int sample;

    uint nextClapPos = 0;

    // Use this for initialization
    void Start () {
        editor = ChartEditor.FindCurrentEditor();
        clapSource = gameObject.AddComponent<AudioSource>();
        initLocalPos = transform.localPosition;

#if BASS_AUDIO
        clapBytes = clap.GetWavBytes();
        sample = Bass.BASS_SampleLoad(clapBytes, 0, clapBytes.Length, 15, BASSFlag.BASS_DEFAULT);
#endif
    }

    // Update is called once per frame
    void Update () {

        // Offset by audio calibration
        Vector3 pos = initLocalPos;
#if BASS_AUDIO
        pos.y += Song.TimeToWorldYPosition(Globals.audioCalibrationMS / 1000.0f * Globals.gameSpeed);
#else
        pos.y += Song.TimeToWorldYPosition(Globals.clapCalibrationMS / 1000.0f * Globals.gameSpeed);
#endif
        transform.localPosition = pos;

        uint currentTickPos = editor.currentSong.WorldYPositionToChartPosition(transform.position.y);
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (currentTickPos >= nextClapPos)
            {
                if (Globals.metronomeActive)
                {
#if BASS_AUDIO
                    int channel = Bass.BASS_SampleGetChannel(sample, false); // get a sample channel
                    if (channel != 0)
                        Bass.BASS_ChannelPlay(channel, false); // play it
                    else
                        Debug.LogError("Clap error: " + Bass.BASS_ErrorGetCode() + ", " + sample);
#else
                    clapSource.PlayOneShot(clap);
#endif
                }

                nextClapPos += (uint)editor.currentSong.resolution;
            }
        }
        else
        {
            // Calculate the starting clap pos
            if (currentTickPos % editor.currentSong.resolution > 0 ? true : false)
                nextClapPos = (currentTickPos / (uint)editor.currentSong.resolution + 1) * (uint)editor.currentSong.resolution;
            else
                nextClapPos = (currentTickPos / (uint)editor.currentSong.resolution) * (uint)editor.currentSong.resolution;
        }
	}
}
