using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetronomePlaybackSystem : SystemManagerState.System
{
    OneShotSampleStream sampleStream;
    uint nextClapPos = 0;

    public MetronomePlaybackSystem(float playFromTime)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song currentSong = editor.currentSong;

        uint currentTickPos = editor.currentSong.TimeToTick(playFromTime, editor.currentSong.resolution);
        if (currentTickPos > 0)
            --currentTickPos;

        nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
    }

    public override void Enter()
    {
        sampleStream = ChartEditor.Instance.sfxAudioStreams.GetSample(SkinKeys.metronome);
        Debug.Assert(sampleStream != null);
    }

    public override void Update()
    {
        ChartEditor editor = ChartEditor.Instance;
        Song currentSong = editor.currentSong;
        AudioStream mainAudio = currentSong.mainSongAudio;

        float currentAudioTime = 0;
        if (mainAudio != null)
        {
            currentAudioTime = mainAudio.CurrentPositionInSeconds();
        }
        else
        {
            float audioStrikelinePos = editor.services.sfxCalibratedStrikelinePos;
            currentAudioTime = TickFunctions.WorldYPositionToTime(audioStrikelinePos);
        }

        uint currentTickPos = editor.currentSong.TimeToTick(currentAudioTime, editor.currentSong.resolution);

        if (currentTickPos >= nextClapPos)
        {
            if (GameSettings.metronomeActive)
            {
                sampleStream.volume = GameSettings.sfxVolume * GameSettings.vol_master;
                sampleStream.pan = GameSettings.audio_pan;
                sampleStream.Play();
            }

            nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
        }
    }

    public override void Exit()
    {
        sampleStream = null;
    }

    uint CalculateNextBeatTickPosition(uint currentTickPosition)
    {
        ChartEditor editor = ChartEditor.Instance;
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
