using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFretManager : HitAnimation
{
    public SpriteRenderer fretBase;
    public SpriteRenderer fretCover;
    public SpriteRenderer fretPress;
    public SpriteRenderer fretRelease;
    public SpriteRenderer toAnimate;
    public SpriteRenderer fretStem;
	
	// Update is called once per frame
	void Update () {
		if (running)
        {
            toAnimate.transform.localPosition = new Vector3(toAnimate.transform.localPosition.x, toAnimate.transform.localPosition.y - SPEED * Time.deltaTime, toAnimate.transform.localPosition.z);
            if (toAnimate.transform.localPosition.y < 0)
            {
                StopAnim();
            }
        }
	}

    public override void StopAnim()
    {
        running = false;
        toAnimate.gameObject.SetActive(false);
        Release();
    }

    public override void PlayOneShot()
    {
        fretPress.gameObject.SetActive(false);
        fretRelease.gameObject.SetActive(false);
        toAnimate.gameObject.SetActive(true);
        toAnimate.transform.localPosition = new Vector3(toAnimate.transform.localPosition.x, START_ANIM_HEIGHT, toAnimate.transform.localPosition.z);

        running = true;
    }

    public override void Press()
    {
        if (!running)
        {
            fretPress.gameObject.SetActive(true);
            fretRelease.gameObject.SetActive(false);
        }
    }

    public override void Release()
    {
        if (!running)
        {
            fretPress.gameObject.SetActive(false);
            fretRelease.gameObject.SetActive(true);
        }
    }
}
