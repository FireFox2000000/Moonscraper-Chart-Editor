﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GameplayManager : MonoBehaviour {
    public CameraShake camShake;

    HitWindowFeeder hitWindowFeeder;
    AudioSource audioSource;
    OneShotSampleStream sample;

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
        editor = ChartEditor.Instance;

        LoadSoundClip();

        initSize = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);

        hitWindowFeeder = gameObject.AddComponent<HitWindowFeeder>();
        guitarGameplayRulestate = new GuitarGameplayRulestate(KickMissFeedback);
        drumsGameplayRulestate = new DrumsGameplayRulestate(KickMissFeedback);

        initialised = true;
        statsPanel.SetActive(false);

        editor.events.editorStateChangedEvent.Register(OnApplicationModeChanged);
        editor.events.chartReloadedEvent.Register(OnChartReloaded);
    }

    void LoadSoundClip()
    {
        if (sample != null)
            sample.Dispose();

        sample = editor.sfxAudioStreams.GetSample(SkinKeys.break0);
        Debug.Assert(sample != null);   // todo, get this when entering playing state
        sample.onlyPlayIfStopped = true;
    }

    void Update()
    {
        // Configure collisions and choose to update the hit window or not
        if (editor.currentState == ChartEditor.State.Playing && !GameSettings.bot)
        {
            transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Gameplay
        if (editor.currentState == ChartEditor.State.Playing && !GameSettings.bot)
        {
            float currentTime = editor.currentVisibleTime;

            if (editor.currentChart.gameMode == Chart.GameMode.Guitar)
            {
                guitarGameplayRulestate.Update(currentTime, hitWindowFeeder.guitarHitWindow);
                UpdateUIStats(guitarGameplayRulestate);
            }
            else if (editor.currentChart.gameMode == Chart.GameMode.Drums)
            {
                drumsGameplayRulestate.Update(currentTime, hitWindowFeeder.drumsHitWindow);
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
        if (sample.Play())
        {
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

    void OnApplicationModeChanged(in ChartEditor.State editorState)
    {
        Reset();
        statsPanel.SetActive(editorState == ChartEditor.State.Playing && !GameSettings.bot);
    }

    void OnChartReloaded()
    {
        Reset();
    }
}
