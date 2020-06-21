///Daniel Moore (Firedan1176) - Firedan1176.webs.com/
///26 Dec 2015
///
///Shakes camera parent object

using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{

    public bool debugMode = false;//Test-run/Call ShakeCamera() on start

    public float shakeAmount;//The amount to shake this frame.
    public float shakeDuration;//The duration this frame.

    bool isRunning = false; //Is the coroutine running right now?

    float initYPos;
    void Start()
    {
        initYPos = transform.localPosition.y;

        ChartEditor.Instance.gameplayEvents.explicitMissEvent.Register(ShakeCamera);

        if (debugMode) ShakeCamera();
    }

    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.R))
            ShakeCamera();

    }


    public void ShakeCamera()
    {
        if (!isRunning) StartCoroutine(Shake());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
    }

    public void ShakeCamera(float amount, float duration)
    {
        /*
        shakeAmount += amount;//Add to the current amount.
        startAmount = shakeAmount;//Reset the start amount, to determine percentage.
        shakeDuration += duration;//Add to the current time.
        startDuration = shakeDuration;//Reset the start time.*/

        if (!isRunning) StartCoroutine(Shake());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
    }


    IEnumerator Shake()
    {
        isRunning = true;
        float startTime = Time.time;
        Vector3 pos;

        while ((Time.time - startTime) < shakeDuration)
        {
            float elapsedTime = Time.time - startTime;
            pos = transform.localPosition;
            pos.y = initYPos + Mathf.Sin(elapsedTime / shakeDuration * Mathf.PI) * shakeAmount;
            transform.localPosition = pos;
            yield return null;
        }

        pos = transform.localPosition;
        pos.y = initYPos;
        transform.localPosition = pos;

        isRunning = false;
    }

}