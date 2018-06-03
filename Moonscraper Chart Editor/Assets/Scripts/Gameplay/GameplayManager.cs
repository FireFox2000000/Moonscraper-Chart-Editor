// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define GAMEPAD
#define MISS_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using Un4seen.Bass;

[RequireComponent(typeof(AudioSource))]
public class GameplayManager : MonoBehaviour {
    public AudioClip comboBreak;
    public CameraShake camShake;
    public HitWindowManager hitWindowManager;

    AudioSource audioSource;
    int sample;
    int channel;

    public GameObject statsPanel;
    public UnityEngine.UI.Text noteStreakText;
    public UnityEngine.UI.Text percentHitText;
    public UnityEngine.UI.Text totalHitText;

    // UI stats
    static uint noteStreak = 0;
    public static uint ns { get { return noteStreak; } }
    uint notesHit = 0;
    uint totalNotes = 0;

    

    ChartEditor editor;
    float initSize;
    bool initialised = false;

    public static GuitarInput guitarInput = new GuitarInput();

    HitWindow<GuitarNoteHitKnowledge> hitWindow
    {
        get { return hitWindowManager.hitWindow; }
    }

    // Rules
    GuitarNoteHitAndMissDetect hitAndMissNoteDetect;
    GuitarSustainBreakDetect sustainBreakDetect;
    GuitarSustainHitKnowledge guitarSustainHitKnowledge;

    void Start()
    {
        byte[] comboBreakBytes = comboBreak.GetWavBytes();
        sample = Bass.BASS_SampleLoad(comboBreakBytes, 0, comboBreakBytes.Length, 1, BASSFlag.BASS_DEFAULT);
        channel = Bass.BASS_SampleGetChannel(sample, false);
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        hitAndMissNoteDetect = new GuitarNoteHitAndMissDetect(HitNote, MissNote);
        sustainBreakDetect = new GuitarSustainBreakDetect(SustainBreak);
        guitarSustainHitKnowledge = new GuitarSustainHitKnowledge();

        initialised = true;
    }

    void Update()
    {
        float currentTime = editor.currentVisibleTime;

        statsPanel.SetActive(Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot);

        guitarInput.Update();

        // Configure collisions and choose to update the hit window or not
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot)
        {
            transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
            
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Gameplay
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot)
        {
            if (editor.currentChart.gameMode == Chart.GameMode.Guitar)
            {
                // Guitar gamemode rulestate
                for (int i = 0; i < hitWindowManager.UpdateHitWindow(noteStreak); ++i)
                {
                    if (noteStreak > 0)
                        Debug.Log("Missed due to note falling out of window");

                    MissNote(currentTime, GuitarNoteHitAndMissDetect.MissSubType.NoteMiss, null);
                }

                guitarSustainHitKnowledge.Update(currentTime);

                hitAndMissNoteDetect.Update(currentTime, hitWindow, guitarInput, noteStreak, guitarSustainHitKnowledge);       
                sustainBreakDetect.Update(currentTime, guitarSustainHitKnowledge, guitarInput, noteStreak);
            }
            else
            {
                Debug.LogError("Gameplay currently does not support this gamemode.");
            }

            UpdateUIStats();
        }
        else
        {
            Reset();
        }
    }

    void UpdateUIStats()
    {
        noteStreakText.text = noteStreak.ToString();
        if (totalNotes > 0)
            percentHitText.text = ((float)notesHit / (float)totalNotes * 100).Round(2).ToString() + "%";
        else
            percentHitText.text = "0.00%";

        totalHitText.text = notesHit.ToString() + " / " + totalNotes.ToString();
    }

    void KickMissFeedback()
    {
        if (Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_STOPPED || Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PAUSED)
        {
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, GameSettings.sfxVolume * GameSettings.vol_master);
            Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_PAN, GameSettings.audio_pan);
            Bass.BASS_ChannelPlay(channel, false); // play it
            camShake.ShakeCamera();
        }
    }

    public void Reset()
    {
        noteStreakText.text = "0";
        percentHitText.text = "0%";
        totalHitText.text = "0/0";
        noteStreak = 0;
        notesHit = 0;
        totalNotes = 0;

        if (initialised)
        {
            hitWindowManager.Reset();
            hitAndMissNoteDetect.Reset();
            sustainBreakDetect.Reset();
            guitarSustainHitKnowledge.Reset();
        }
    }

    void HitNote(float time, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        // Force the note out of the window
        noteHitKnowledge.hasBeenHit = true;
        noteHitKnowledge.shouldExitWindow = true;

        Note note = noteHitKnowledge.note;

        ++noteStreak;
        ++notesHit;
        ++totalNotes;

        foreach (Note chordNote in note.GetChord())
        {
            chordNote.controller.hit = true;
            chordNote.controller.PlayIndicatorAnim();
        }

        if (note.sustain_length > 0 && note.controller)
            guitarSustainHitKnowledge.Add(note);
    }

    void MissNote(float time, GuitarNoteHitAndMissDetect.MissSubType missSubType, GuitarNoteHitKnowledge noteHitKnowledge)
    {
        if (noteStreak > 10)
        {
            KickMissFeedback();
        }

        noteStreak = 0;

        if (missSubType == GuitarNoteHitAndMissDetect.MissSubType.NoteMiss)
            ++totalNotes;

        if (noteHitKnowledge != null)
        {
            noteHitKnowledge.hasBeenHit = true; // Don't want to count this as a miss twice when it gets removed from the window
            noteHitKnowledge.shouldExitWindow = true;
        }
    }

    void SustainBreak(float time, Note note)
    {
        foreach (Note chordNote in note.GetChord())
        {
            if (chordNote.controller)
                chordNote.controller.sustainBroken = true;
            else
                Debug.LogError("Trying to break the sustain of a note without a controller");
        }
    }

    ~GameplayManager()
    {
        Bass.BASS_SampleFree(sample);
    }
}
