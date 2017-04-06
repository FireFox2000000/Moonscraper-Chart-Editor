using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HitAnimation : MonoBehaviour {
    [HideInInspector]
    public bool running = false;

    public abstract void PlayOneShot();
    public abstract void StopAnim();
    public abstract void Press();
    public abstract void Release();
}
