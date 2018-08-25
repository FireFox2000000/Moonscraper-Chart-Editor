// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define BASS_AUDIO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Un4seen.Bass;

public class Metronome : UpdateableService {
    ChartEditor editor;
#if !BASS_AUDIO
    AudioSource clapSource;
#endif
    Vector3 initLocalPos;

    public AudioClip clap;
    static byte[] clapBytes;
    static int sample;

    uint nextClapPos = 0;

    // Use this for initialization
    protected override void Start () {
        editor = ChartEditor.GetInstance();      
        initLocalPos = transform.localPosition;

#if BASS_AUDIO
        clapBytes = clap.GetWavBytes();
        sample = Bass.BASS_SampleLoad(clapBytes, 0, clapBytes.Length, 15, BASSFlag.BASS_DEFAULT);
#else
        clapSource = gameObject.AddComponent<AudioSource>();
#endif

        base.Start();
    }

    // Update is called once per frame
    public override void OnServiceUpdate()
    {
        // Offset by audio calibration
        Vector3 pos = initLocalPos;
#if BASS_AUDIO
        pos.y += TickFunctions.TimeToWorldYPosition(GameSettings.audioCalibrationMS / 1000.0f * GameSettings.gameSpeed);
#else
        pos.y += Song.TimeToWorldYPosition(Globals.clapCalibrationMS / 1000.0f * Globals.gameSpeed);
#endif
        transform.localPosition = pos;

        uint currentTickPos = editor.currentSong.WorldYPositionToTick(transform.position.y);

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (currentTickPos >= nextClapPos)
            {
                if (GameSettings.metronomeActive)
                {
#if BASS_AUDIO
                    int channel = Bass.BASS_SampleGetChannel(sample, false); // get a sample channel
                    if (channel != 0)
                    {
                        Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, GameSettings.sfxVolume * GameSettings.vol_master);
                        Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, GameSettings.audio_pan);
                        Bass.BASS_ChannelPlay(channel, false); // play it
                    }
                    else
                        Debug.LogError("Clap error: " + Bass.BASS_ErrorGetCode() + ", " + sample);
#else
                    clapSource.PlayOneShot(clap);
#endif
                }
            }
        }

        nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
    }

    uint CalculateNextBeatTickPosition(uint currentTickPosition)
    {
        Song song = editor.currentSong;
        TimeSignature[] timeSignatures = editor.currentSong.timeSignatures;
        uint standardMeasureLengthTicks = (uint)(Song.RESOLUTIONS_PER_MEASURE * song.resolution);

        int lastTsIndex = SongObjectHelper.FindClosestPositionRoundedDown(currentTickPosition, timeSignatures);
        TimeSignature currentTimeSignature = timeSignatures[lastTsIndex];
        TimeSignature nextTimeSignature = lastTsIndex + 1 < timeSignatures.Length ? timeSignatures[lastTsIndex + 1] : null;
        uint tickOrigin = currentTimeSignature.tick;
        float beatDeltaTick = standardMeasureLengthTicks / currentTimeSignature.beatsPerMeasure;

        if (nextTimeSignature != null && currentTickPosition + beatDeltaTick >= nextTimeSignature.tick)
            return nextTimeSignature.tick;

        uint deltaTick = currentTickPosition - tickOrigin;
        uint remainder = (uint)Mathf.Round(deltaTick % beatDeltaTick);

        return tickOrigin + deltaTick - remainder + (uint)Mathf.Round(beatDeltaTick);
    }

#if BASS_AUDIO
    ~Metronome()
    {
        Bass.BASS_SampleFree(sample);
    }
#endif
}
