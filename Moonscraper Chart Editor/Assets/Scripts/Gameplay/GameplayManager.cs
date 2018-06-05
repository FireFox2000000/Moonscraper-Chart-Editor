// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Un4seen.Bass;

[RequireComponent(typeof(AudioSource))]
public class GameplayManager : MonoBehaviour {
    public AudioClip comboBreak;
    public CameraShake camShake;

    HitWindowFeeder hitWindowFeeder;
    AudioSource audioSource;
    int sample;
    int channel;

    public GameObject statsPanel;
    public UnityEngine.UI.Text noteStreakText;
    public UnityEngine.UI.Text percentHitText;
    public UnityEngine.UI.Text totalHitText;

    ChartEditor editor;
    float initSize;
    bool initialised = false;

    public static GamepadInput mainGamepad = new GamepadInput();

    GuitarGameplayRulestate guitarGameplayRulestate;
    DrumsGameplayRulestate drumsGameplayRulestate;

    void Start()
    {
        byte[] comboBreakBytes = comboBreak.GetWavBytes();
        sample = Bass.BASS_SampleLoad(comboBreakBytes, 0, comboBreakBytes.Length, 1, BASSFlag.BASS_DEFAULT);
        channel = Bass.BASS_SampleGetChannel(sample, false);
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);

        hitWindowFeeder = gameObject.AddComponent<HitWindowFeeder>();
        guitarGameplayRulestate = new GuitarGameplayRulestate(KickMissFeedback);
        drumsGameplayRulestate = new DrumsGameplayRulestate(KickMissFeedback);

        initialised = true;
    }

    void Update()
    {
        float currentTime = editor.currentVisibleTime;

        statsPanel.SetActive(Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot);

        mainGamepad.Update();

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
                guitarGameplayRulestate.Update(currentTime, hitWindowFeeder.guitarHitWindow, mainGamepad);
                UpdateUIStats(guitarGameplayRulestate);
            }
            else if (editor.currentChart.gameMode == Chart.GameMode.Drums)
            {
                drumsGameplayRulestate.Update(currentTime, hitWindowFeeder.drumsHitWindow, mainGamepad);
                UpdateUIStats(drumsGameplayRulestate);
            }
            else
            {
                Debug.LogError("Gameplay currently does not support this gamemode.");
            }
        }
        else
        {
            Reset();
        }
    }

    void UpdateUIStats(BaseGameplayRulestate currentRulestate)
    {
        BaseGameplayRulestate.NoteStats stats = currentRulestate.stats;
        uint noteStreak = stats.noteStreak;
        uint totalNotes = stats.totalNotes;
        uint notesHit = stats.notesHit;

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

        if (initialised)
        {
            hitWindowFeeder.Reset();
            guitarGameplayRulestate.Reset();
            drumsGameplayRulestate.Reset();
        }
    }

    ~GameplayManager()
    {
        Bass.BASS_SampleFree(sample);
    }
}
