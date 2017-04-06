using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFretManager : HitAnimation
{
    public GameObject fretPress;
    public GameObject fretRelease;
    public GameObject toAnimate;

    const float SPEED = 3;
	
	// Update is called once per frame
	void Update () {
		if (running)
        {
            toAnimate.transform.position = new Vector3(toAnimate.transform.position.x, toAnimate.transform.position.y - SPEED * Time.deltaTime, toAnimate.transform.position.z);
            if (toAnimate.transform.position.y < 0)
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
