// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFretManager : HitAnimation
{
    public bool canUse = false;

    public SpriteRenderer fretBaseRen;
    public SpriteRenderer fretCoverRen;
    public SpriteRenderer fretPressRen;
    public SpriteRenderer fretReleaseRen;
    public SpriteRenderer toAnimateRen;
    public SpriteRenderer fretStemRen;

    [HideInInspector] public Sprite fretBase = null;
    [HideInInspector] public Sprite fretCover = null;
    [HideInInspector] public Sprite fretPress = null;
    [HideInInspector] public Sprite fretRelease = null;
    [HideInInspector] public Sprite toAnimate = null;

    [HideInInspector] public Sprite drumFretBase = null;
    [HideInInspector] public Sprite drumFretCover = null;
    [HideInInspector] public Sprite drumFretPress = null;
    [HideInInspector] public Sprite drumFretRelease = null;
    [HideInInspector] public Sprite drumToAnimate = null;

    void Start()
    {
        SetStandardFrets();

        ChartEditor.Instance.events.chartReloadedEvent.Register(SetFrets);
    }

    void OnEnable()
    {
        canUse = true;
    }

    // Update is called once per frame
    void Update () {
		if (running)
        {
            toAnimateRen.transform.localPosition = new Vector3(toAnimateRen.transform.localPosition.x, toAnimateRen.transform.localPosition.y - SPEED * Time.deltaTime, toAnimateRen.transform.localPosition.z);
            if (toAnimateRen.transform.localPosition.y < 0)
            {
                StopAnim();
            }
        }
    }

    public void SetFrets()
    {
        if (Globals.drumMode)
            SetDrumFrets();
        else
            SetStandardFrets();
    }

    void SetStandardFrets()
    {
        fretBaseRen.sprite = fretBase;
        fretCoverRen.sprite = fretCover;
        fretPressRen.sprite = fretPress;
        fretReleaseRen.sprite = fretRelease;
        toAnimateRen.sprite = toAnimate;
    }

    void SetDrumFrets()
    {
        if (drumFretBase != null)
            fretBaseRen.sprite = drumFretBase;
        else
            fretBaseRen.sprite = fretBase;

        if (drumFretCover != null)
            fretCoverRen.sprite = drumFretCover;
        else
            fretCoverRen.sprite = fretCover;

        if (drumFretPress != null)
            fretPressRen.sprite = drumFretPress;
        else
            fretPressRen.sprite = fretPress;

        if (drumFretRelease != null)
            fretReleaseRen.sprite = drumFretRelease;
        else
            fretReleaseRen.sprite = fretRelease;

        if (drumToAnimate != null)
            toAnimateRen.sprite = drumToAnimate;
        else
            toAnimateRen.sprite = toAnimate;
    }

    public override void StopAnim()
    {
        running = false;
        toAnimateRen.gameObject.SetActive(false);
        Release();
    }

    public override void PlayOneShot()
    {
        fretPressRen.gameObject.SetActive(false);
        fretReleaseRen.gameObject.SetActive(false);
        toAnimateRen.gameObject.SetActive(true);
        toAnimateRen.transform.localPosition = new Vector3(toAnimateRen.transform.localPosition.x, START_ANIM_HEIGHT, toAnimateRen.transform.localPosition.z);

        running = true;
    }

    public override void Press()
    {
        if (!running)
        {
            fretPressRen.gameObject.SetActive(true);
            fretReleaseRen.gameObject.SetActive(false);
        }
    }

    public override void Release()
    {
        if (!running)
        {
            fretPressRen.gameObject.SetActive(false);
            fretReleaseRen.gameObject.SetActive(true);
        }
    }
}
