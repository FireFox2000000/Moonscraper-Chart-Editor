using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LyricEditor2AutoScroller : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform endSpacer;


    void Start () {

    }

    void Update () {
        // Move end spacer to bottom of scroll view
        endSpacer.SetAsLastSibling();
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
}
