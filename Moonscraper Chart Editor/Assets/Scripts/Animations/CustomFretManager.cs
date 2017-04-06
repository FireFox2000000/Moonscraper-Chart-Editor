using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFretManager : HitAnimation
{
    public GameObject fretPress;
    public GameObject fretRelease;
    public GameObject toAnimate;
	
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
        toAnimate.SetActive(false);
        Release();
    }

    public override void PlayOneShot()
    {
        fretPress.SetActive(false);
        fretRelease.SetActive(false);
        toAnimate.SetActive(true);
        toAnimate.transform.localPosition = new Vector3(toAnimate.transform.localPosition.x, START_ANIM_HEIGHT, toAnimate.transform.localPosition.z);

        running = true;
    }

    public override void Press()
    {
        if (!running)
        {
            fretPress.SetActive(true);
            fretRelease.SetActive(false);
        }
    }

    public override void Release()
    {
        if (!running)
        {
            fretPress.SetActive(false);
            fretRelease.SetActive(true);
        }
    }
}
