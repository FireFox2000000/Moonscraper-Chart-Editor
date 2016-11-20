using UnityEngine;
using System.Collections;

static class Utility {
    public const int NOTFOUND = -1;  
}

public static class floatExtension
{
    public static float Round(this float sourceFloat, int decimalPlaces)
    {
        float places = Mathf.Pow(10, decimalPlaces);
        return Mathf.Round((sourceFloat * places) / places);
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
