// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class StrikelineAudioController : MonoBehaviour {

    public AudioClip clap;
    static float lastClapPos = -1;
    public static float startYPoint = -1;
    Vector3 initLocalPos;

    OneShotSampleStream sample;

    void Start()
    {
        LoadSoundClip();
        initLocalPos = transform.localPosition;  
    }

    void LoadSoundClip()
    {
        if (sample != null)
            sample.Dispose();

        AudioClip currentClap = SkinManager.Instance.GetSkinItem(SkinKeys.clap, clap);
        sample = AudioManager.LoadSampleStream(currentClap, 50);
    }

    void Update()
    {
        Vector3 pos = initLocalPos;
        pos.y += 0.02f * GameSettings.hyperspeed / GameSettings.gameSpeed;
        transform.localPosition = pos;

        if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            lastClapPos = -1;
    }

    public void Clap(float worldYPos)
    {
        if (Globals.applicationMode != Globals.ApplicationMode.Playing)
            lastClapPos = -1;

        if (worldYPos > lastClapPos && worldYPos >= startYPoint)
        {
            sample.volume = GameSettings.sfxVolume * GameSettings.vol_master;
            sample.pan = GameSettings.audio_pan;
            sample.Play();
        }
        lastClapPos = worldYPos;
    }
}
