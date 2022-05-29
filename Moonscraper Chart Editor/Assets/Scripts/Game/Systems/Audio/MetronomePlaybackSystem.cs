// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class MetronomePlaybackSystem : SystemManagerState.System
{
    OneShotSampleStream sampleStream;
    uint nextClapPos = 0;

    public MetronomePlaybackSystem()
    {
    }

    public override void SystemEnter()
    {
        ChartEditor editor = ChartEditor.Instance;

        sampleStream = editor.sfxAudioStreams.GetSample(SkinKeys.metronome);
        Debug.Assert(sampleStream != null);

        float currentAudioTime = editor.services.sfxAudioTime;
        Song currentSong = editor.currentSong;

        uint currentTickPos = editor.currentSong.TimeToTick(currentAudioTime, editor.currentSong.resolution);
        if (currentTickPos > 0)
            --currentTickPos;

        nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
    }

    public override void SystemUpdate()
    {
        ChartEditor editor = ChartEditor.Instance;
        Song currentSong = editor.currentSong;

        float currentAudioTime = editor.services.sfxAudioTime;
        uint currentTickPos = editor.currentSong.TimeToTick(currentAudioTime, editor.currentSong.resolution);

        if (currentTickPos >= nextClapPos)
        {
            if (Globals.gameSettings.metronomeActive)
            {
                sampleStream.volume = Globals.gameSettings.sfxVolume * Globals.gameSettings.vol_master;
                sampleStream.pan = Globals.gameSettings.audio_pan;
                sampleStream.Play();
            }

            nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
        }
    }

    public override void SystemExit()
    {
        sampleStream = null;
    }

    uint CalculateNextBeatTickPosition(uint currentTickPosition)
    {
        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;
        var timeSignatures = editor.currentSong.timeSignatures;
        uint standardMeasureLengthTicks = (uint)(SongConfig.RESOLUTIONS_PER_MEASURE * song.resolution);

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
