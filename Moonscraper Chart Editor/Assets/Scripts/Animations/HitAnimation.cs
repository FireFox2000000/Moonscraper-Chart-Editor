using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HitAnimation : MonoBehaviour {
    [HideInInspector]
    public bool running = false;

    protected const float SPEED = 3;
    protected const float START_ANIM_HEIGHT = 0.2f;

    public abstract void PlayOneShot();
    public abstract void StopAnim();
    public abstract void Press();
    public abstract void Release();
}
