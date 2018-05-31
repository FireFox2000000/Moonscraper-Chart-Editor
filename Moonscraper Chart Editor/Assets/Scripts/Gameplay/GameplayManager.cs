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

    const float FREESTRUM_TIME = 0.2f;

    public GameObject statsPanel;
    public UnityEngine.UI.Text noteStreakText;
    public UnityEngine.UI.Text percentHitText;
    public UnityEngine.UI.Text totalHitText;

    // UI stats
    static uint noteStreak = 0;
    public static uint ns { get { return noteStreak; } }
    uint notesHit = 0;
    uint totalNotes = 0;

    List<NoteController> currentSustains = new List<NoteController>();

    ChartEditor editor;
    float initSize;
    bool initialised = false;

#if GAMEPAD
    public static GamePadState? gamepad;
    public static GamePadState? previousGamepad;
#endif

    HitWindow hitWindow
    {
        get { return hitWindowManager.hitWindow; }
    }

    GuitarNoteHitAndMissDetect hitAndMissNoteDetect;

    void Start()
    {
        byte[] comboBreakBytes = comboBreak.GetWavBytes();
        sample = Bass.BASS_SampleLoad(comboBreakBytes, 0, comboBreakBytes.Length, 1, BASSFlag.BASS_DEFAULT);
        channel = Bass.BASS_SampleGetChannel(sample, false);
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        hitAndMissNoteDetect = new GuitarNoteHitAndMissDetect(HitNote, MissNote);

        initialised = true;
    }

    void Update()
    {
        float currentTime = TickFunctions.WorldYPositionToTime(editor.visibleStrikeline.position.y);

        statsPanel.SetActive(Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot);

        // Update the Xbox 360 state
#if GAMEPAD
        previousGamepad = gamepad;
        gamepad = null;

        for (int i = 0; i < 1; ++i)
        {
            GamePadState testState = GamePad.GetState((PlayerIndex)i);
            if (testState.IsConnected)
            {
                gamepad = testState;
                break;
            }
        }
#endif
        // Configure collisions and choose to update the hit window or not
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot)
        {
            transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
            for (int i = 0; i < hitWindowManager.UpdateHitWindow(noteStreak); ++i)
            {
                MissNote(currentTime, GuitarNoteHitAndMissDetect.MissSubType.NoteMiss);
                if (noteStreak > 0)
                    Debug.Log("Missed due to note falling out of window");
            }
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Gameplay
        if (Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot)
        {
            uint startNS = noteStreak;

            hitAndMissNoteDetect.Update(currentTime, hitWindow, gamepad, noteStreak);

            UpdateSustainBreaking();
            UpdateUIStats();
            UpdateMissFeedback(startNS);
        }
        else
        {
            Reset();
        }
    }

    void UpdateSustainBreaking()
    {
        int inputMask = GameplayInputFunctions.GetFretInputMask(gamepad);

        foreach (NoteController note in currentSustains.ToArray())
        {
            if (!note.gameObject.activeSelf || note.note == null)
            {
                currentSustains.Remove(note);
                continue;
            }
            if (!note.isActivated &&
                (
                    noteStreak == 0
                    ||
                    (
                        !note.note.IsChord && !GameplayInputFunctions.ValidateFrets(note.note, inputMask, noteStreak))
                        || (note.note.IsChord && note.note.mask != inputMask)
                    )
                )
            {
                foreach (Note chordNote in note.note.GetChord())
                    chordNote.controller.sustainBroken = true;
                currentSustains.Remove(note);
            }
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

    void UpdateMissFeedback(uint frameStartNS)
    {
        if (frameStartNS >= 10 && noteStreak < frameStartNS &&
                    (
                        Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_STOPPED || Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PAUSED
                    )
                )
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
            currentSustains.Add(note.controller);
    }

    void MissNote(float time, GuitarNoteHitAndMissDetect.MissSubType missSubType)
    {
        noteStreak = 0;

        if (missSubType == GuitarNoteHitAndMissDetect.MissSubType.NoteMiss)
            ++totalNotes;
    }

    ~GameplayManager()
    {
        Bass.BASS_SampleFree(sample);
    }
}
