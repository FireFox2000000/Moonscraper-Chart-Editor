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
        statsPanel.SetActive(false);

        TriggerManager.onApplicationModeChangedTriggerList.Add(OnApplicationModeChanged);
        TriggerManager.onChartReloadTriggerList.Add(OnChartReloaded);
    }

    void Update()
    {
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
            float currentTime = editor.currentVisibleTime;
            GamepadInput gamepad = editor.inputManager.mainGamepad;

            if (editor.currentChart.gameMode == Chart.GameMode.Guitar)
            {
                guitarGameplayRulestate.Update(currentTime, hitWindowFeeder.guitarHitWindow, gamepad);
                UpdateUIStats(guitarGameplayRulestate);
            }
            else if (editor.currentChart.gameMode == Chart.GameMode.Drums)
            {
                drumsGameplayRulestate.Update(currentTime, hitWindowFeeder.drumsHitWindow, gamepad);
                UpdateUIStats(drumsGameplayRulestate);
            }
            else
            {
                Debug.LogError("Gameplay currently does not support this gamemode.");
            }
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

    void Reset()
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

    void OnApplicationModeChanged(Globals.ApplicationMode applicationMode)
    {
        Reset();
        statsPanel.SetActive(Globals.applicationMode == Globals.ApplicationMode.Playing && !GameSettings.bot);
    }

    void OnChartReloaded()
    {
        Reset();
    }

    ~GameplayManager()
    {
        Bass.BASS_SampleFree(sample);
    }
}
