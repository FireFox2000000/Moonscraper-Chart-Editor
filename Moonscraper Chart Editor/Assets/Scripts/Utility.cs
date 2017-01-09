using UnityEngine;
using System.Collections;
using System;

static class Utility {
    public const int NOTFOUND = -1;

    public static string timeConvertion(float time)
    {
        System.TimeSpan levelTime = System.TimeSpan.FromSeconds(time);

        string format = string.Empty;
        if (time < 0)
            format += "-";

        format += "{0:D2}:{1:D2}:{2:D2}";

        return string.Format(format,
                Mathf.Abs(levelTime.Minutes),
                Mathf.Abs(levelTime.Seconds),
                millisecondRounding(Mathf.Abs(levelTime.Milliseconds), 2));
    }

    static int millisecondRounding(int value, int roundPlaces)
    {
        string sVal = value.ToString();

        if (sVal.Length > 0 && sVal[0] == '-')
            ++roundPlaces;

        if (sVal.Length > roundPlaces)
            sVal = sVal.Remove(roundPlaces);

        return int.Parse(sVal);
    }

    public static bool validateExtension(string filepath, string[] validExtensions)
    {
        // Need to check extension
        string extension = System.IO.Path.GetExtension(filepath);

        foreach (string validExtension in validExtensions)
        {
            if (extension == validExtension)
                return true;
        }
        return false;
    }
}

public static class floatExtension
{
    public static float Round(this float sourceFloat, int decimalPlaces)
    {
        return (float)Math.Round(sourceFloat, decimalPlaces);
        //float places = Mathf.Pow(10, decimalPlaces);
        //return Mathf.Round(sourceFloat * places) / places;

    }
}

public static class RectTransformExtension
{
    public static Vector2 GetScreenPosition(this RectTransform source)
    {
        return RectTransformUtility.WorldToScreenPoint(null, source.transform.position);
    }

    public static Rect GetScreenCorners(this RectTransform source)
    {
        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        source.GetWorldCorners(corners);

        screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        //screenCorners[0].y = Screen.height - screenCorners[0].y;
        //screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }
}
