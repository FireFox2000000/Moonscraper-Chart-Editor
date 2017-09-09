using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFretManager : HitAnimation
{
    public SpriteRenderer fretBaseRen;
    public SpriteRenderer fretCoverRen;
    public SpriteRenderer fretPressRen;
    public SpriteRenderer fretReleaseRen;
    public SpriteRenderer toAnimateRen;
    public SpriteRenderer fretStemRen;


    public Sprite fretBase = null;

    public Sprite fretCover = null;

    public Sprite fretPress = null;

    public Sprite fretRelease = null;

    public Sprite toAnimate = null;


    public Sprite drumFretBase = null;

    public Sprite drumFretCover = null;

    public Sprite drumFretPress = null;

    public Sprite drumFretRelease = null;

    public Sprite drumToAnimate = null;

    bool prevDrumMode = false;

    void Start()
    {
        SetStandardFrets();
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

        if (Globals.drumMode != prevDrumMode)
        {
            if (Globals.drumMode)
                SetDrumFrets();
            else
                SetStandardFrets();
        }

        prevDrumMode = Globals.drumMode;
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

        if (drumFretCover != null)
            fretCoverRen.sprite = drumFretCover;

        if (drumFretPress != null)
            fretPressRen.sprite = drumFretPress;

        if (drumFretRelease != null)
            fretReleaseRen.sprite = drumFretRelease;

        if (drumToAnimate != null)
            toAnimateRen.sprite = drumToAnimate;
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
