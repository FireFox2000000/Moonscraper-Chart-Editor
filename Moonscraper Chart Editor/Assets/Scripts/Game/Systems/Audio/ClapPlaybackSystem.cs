// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class ClapPlaybackSystem : SystemManagerState.System
{
    OneShotSampleStream sampleStream;

    SongObjectTracker<Note> noteTracker;
    SongObjectTracker<Starpower> spTracker;
    SongObjectTracker<ChartEvent> chartEventTracker;

    SongObjectTracker<BPM> bpmTracker;
    SongObjectTracker<TimeSignature> tsTracker;
    SongObjectTracker<Section> sectionTracker;
    SongObjectTracker<MoonscraperChartEditor.Song.Event> eventsTracker;

    static readonly Dictionary<SongObject.ID, GameSettings.ClapToggle> s_songObjectIdToClapOption = new Dictionary<SongObject.ID, GameSettings.ClapToggle>()
    {
        // Notes are subdivided and are not applicable here
        { SongObject.ID.Starpower,      GameSettings.ClapToggle.STARPOWER },
        { SongObject.ID.ChartEvent,     GameSettings.ClapToggle.CHARTEVENT },
        { SongObject.ID.BPM,            GameSettings.ClapToggle.BPM },
        { SongObject.ID.TimeSignature,  GameSettings.ClapToggle.TS },
        { SongObject.ID.Event,          GameSettings.ClapToggle.EVENT },
        { SongObject.ID.Section,        GameSettings.ClapToggle.SECTION },
    };

    float playFromTime = 0;

    public ClapPlaybackSystem(float playFromTime)
    {
        // ignore claps that occur before the play point
        this.playFromTime = playFromTime;
    }

    public override void SystemEnter()
    {
        sampleStream = ChartEditor.Instance.sfxAudioStreams.GetSample(SkinKeys.clap);
        Debug.Assert(sampleStream != null && sampleStream.isValid);

        float currentAudioTime = playFromTime;

        ChartEditor editor = ChartEditor.Instance;
        Song currentSong = editor.currentSong;

        uint currentTick = currentSong.TimeToTick(currentAudioTime, currentSong.resolution);

        noteTracker = new SongObjectTracker<Note>(ChartEditor.Instance.currentChart.notes, currentTick);
        spTracker = new SongObjectTracker<Starpower>(ChartEditor.Instance.currentChart.starPower, currentTick);
        chartEventTracker = new SongObjectTracker<ChartEvent>(ChartEditor.Instance.currentChart.events, currentTick);
        bpmTracker = new SongObjectTracker<BPM>(ChartEditor.Instance.currentSong.bpms, currentTick);
        tsTracker = new SongObjectTracker<TimeSignature>(ChartEditor.Instance.currentSong.timeSignatures, currentTick);
        sectionTracker = new SongObjectTracker<Section>(ChartEditor.Instance.currentSong.sections, currentTick);
        eventsTracker = new SongObjectTracker<MoonscraperChartEditor.Song.Event >(ChartEditor.Instance.currentSong.events, currentTick);
    }

    public override void SystemUpdate()
    {
        ChartEditor editor = ChartEditor.Instance;
        Song currentSong = editor.currentSong;

        float currentAudioTime = editor.services.sfxAudioTime;

        uint currentTick = currentSong.TimeToTick(currentAudioTime, currentSong.resolution);
        bool hasClapped = false;
        bool clapEnabled = Globals.gameSettings.clapEnabled;
        hasClapped |= UpdateClapForTracker(currentTick, noteTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, spTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, chartEventTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, bpmTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, tsTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, sectionTracker, !hasClapped && clapEnabled);
        hasClapped |= UpdateClapForTracker(currentTick, eventsTracker, !hasClapped && clapEnabled);
    }

    bool UpdateClapForTracker<T>(uint currentTick, SongObjectTracker<T> tracker, bool clapSettingEnabled) where T : SongObject
    {
        if (tracker.currentIndex < tracker.objects.Count)
        {
            SongObject so = tracker.objects[tracker.currentIndex];
            bool shouldClap = false;

            // Advance tracker to the next object that is ahead of the current tick
            while (tracker.currentIndex < tracker.objects.Count && tracker.objects[tracker.currentIndex].tick <= currentTick)
            {
                ++tracker.currentIndex;
                shouldClap = true;
            }

            if (clapSettingEnabled && ObjectCanClap(so) && shouldClap)
            {
                sampleStream.volume = Globals.gameSettings.sfxVolume * Globals.gameSettings.vol_master;
                sampleStream.pan = Globals.gameSettings.audio_pan;
                sampleStream.Play();

                return true;
            }
        }

        return false;
    }

    bool ObjectCanClap(SongObject songObject)
    {
        SongObject.ID id = (SongObject.ID)songObject.classID;
        GameSettings.ClapToggle toggleValue;

        // Must have some kind of visual representation on screen
        if (songObject.controller == null || !songObject.controller.isActiveAndEnabled)
            return false;

        bool playClap = false;

        if (s_songObjectIdToClapOption.TryGetValue(id, out toggleValue))
        {
            if ((Globals.gameSettings.clapProperties & toggleValue) != 0)
                playClap = true;
        }
        else if (id == SongObject.ID.Note)
        {
            Note note = songObject as Note;
            Note.NoteType noteType = NoteVisualsManager.GetVisualNoteType(note);

            switch (noteType)
            {
                case Note.NoteType.Strum:
                case Note.NoteType.Cymbal:
                    playClap = (Globals.gameSettings.clapProperties & GameSettings.ClapToggle.STRUM) != 0;
                    break;

                case Note.NoteType.Hopo:
                    playClap = (Globals.gameSettings.clapProperties & GameSettings.ClapToggle.HOPO) != 0;
                    break;

                case Note.NoteType.Tap:
                    playClap = (Globals.gameSettings.clapProperties & GameSettings.ClapToggle.TAP) != 0;
                    break;

                default:
                    break;
            }
        }

        return playClap;
    }

    class SongObjectTracker<T> where T : SongObject
    {
        public SongObjectCache<T> objects;
        public int currentIndex = 0;

        public SongObjectTracker(SongObjectCache<T> objects, uint startTick)
        {
            this.objects = objects;

            // Skip ahead to where we're actually going to start
            int startIndex = SongObjectHelper.FindClosestPositionRoundedDown(startTick, objects);
            if (startIndex != SongObjectHelper.NOTFOUND)
            {
                currentIndex = startIndex;

                if (objects[startIndex].tick < startTick)
                    ++currentIndex;
            }
        }
    }
}
