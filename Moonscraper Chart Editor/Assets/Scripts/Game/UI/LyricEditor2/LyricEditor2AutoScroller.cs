using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LyricEditor2AutoScroller : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform endSpacer;
    [SerializeField]
    float scrollTime;

    float currentDeltaTime = 0;
    float lastY = 0;
    float targetY;


    void Start () {

    }

    void Update () {
        // Move end spacer to bottom of scroll view
        endSpacer.SetAsLastSibling();
        // Update current time
        currentDeltaTime += Time.deltaTime;
        // Scroll to next frame, if needed
        AutoScroll();
    }

    void Enable () {
        endSpacer.gameObject.SetActive(true);
        scrollRect.verticalScrollbar.enabled = false;
        lastY = 0;
    }

    void Disable () {
        endSpacer.gameObject.SetActive(false);
        scrollRect.verticalScrollbar.enabled = true;
    }

    public void SetActive (bool active) {
        if (active) {
            Enable();
        } else {
            Disable();
        }
    }

    // Smoothly interpolate between two values following the trajectory y=2x-x^2
    // for 0<x<1
    static float smoothInterp (float min, float max, float factor) {
        if (factor < 0) {
            return min;
        } else if (factor > 1) {
            return max;
        } else {
            return min + (2 - factor) * factor * (max - min);
        }
    }

    // Scroll to a specific RectTransform
    public void ScrollTo(RectTransform target) {
        // Never mind, DON'T ignore identical scroll calls; this just causes
        // problems with ambiguity because there is no feedback for whether
        // the operation succeeded
        currentDeltaTime = 0;
        lastY = targetY;
        if (target != null) {
            targetY = target.anchoredPosition.y;
        } else {
            targetY = endSpacer.anchoredPosition.y;
        }
    }

    // Scroll to the target position every frame, if needed
    void AutoScroll () {
        float scrollFactor = currentDeltaTime / scrollTime;
        float frameTargetY = smoothInterp(lastY, targetY, scrollFactor);
        float frameTargetScroll = 1 - frameTargetY / endSpacer.anchoredPosition.y;
        if (endSpacer.anchoredPosition.y != 0) {
            scrollRect.verticalNormalizedPosition = frameTargetScroll;
        } else {
            scrollRect.verticalNormalizedPosition = 1;
        }
    }
}
