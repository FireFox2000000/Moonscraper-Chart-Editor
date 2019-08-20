// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {
    ChartEditor editor;

    OneShotSampleStream sampleStream;

    uint nextClapPos = 0;
    readonly string defaultPath = System.IO.Path.Combine(Application.streamingAssetsPath, "SFX/metronome.wav");

    // Use this for initialization
    protected void Start () {
        editor = ChartEditor.Instance;      

        LoadSoundClip();
    }

    void LoadSoundClip()
    {
        if (sampleStream != null)
            sampleStream.Dispose();

        string customPath = SkinManager.Instance.GetSkinItemFilepath(SkinKeys.metronome);
        string currentSFX = string.IsNullOrEmpty(customPath) ? defaultPath : customPath;
        sampleStream = AudioManager.LoadSampleStream(currentSFX, 15);
    }

    // Update is called once per frame
    public void Update()
    {
        // Offset by audio calibration
        float audioStrikelinePos = editor.services.sfxCalibratedStrikelinePos;
        uint currentTickPos = editor.currentSong.WorldYPositionToTick(audioStrikelinePos);

        if (editor.currentState == ChartEditor.State.Playing)
        {
            if (currentTickPos >= nextClapPos)
            {
                if (GameSettings.metronomeActive)
                {
                    sampleStream.volume = GameSettings.sfxVolume * GameSettings.vol_master;
                    sampleStream.pan = GameSettings.audio_pan;
                    sampleStream.Play();
                }
            }
        }

        nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
    }

    uint CalculateNextBeatTickPosition(uint currentTickPosition)
    {
        Song song = editor.currentSong;
        var timeSignatures = editor.currentSong.timeSignatures;
        uint standardMeasureLengthTicks = (uint)(Song.RESOLUTIONS_PER_MEASURE * song.resolution);

        int lastTsIndex = SongObjectHelper.FindClosestPositionRoundedDown(currentTickPosition, timeSignatures);
        TimeSignature currentTimeSignature = timeSignatures[lastTsIndex];
        TimeSignature nextTimeSignature = lastTsIndex + 1 < timeSignatures.Count ? timeSignatures[lastTsIndex + 1] : null;
        uint tickOrigin = currentTimeSignature.tick;
        float beatDeltaTick = standardMeasureLengthTicks / currentTimeSignature.beatsPerMeasure;

        if (nextTimeSignature != null && currentTickPosition + beatDeltaTick >= nextTimeSignature.tick)
            return nextTimeSignature.tick;

        uint deltaTick = currentTickPosition - tickOrigin;
        uint remainder = (uint)Mathf.Round(deltaTick % beatDeltaTick);

        return tickOrigin + deltaTick - remainder + (uint)Mathf.Round(beatDeltaTick);
    }
}
