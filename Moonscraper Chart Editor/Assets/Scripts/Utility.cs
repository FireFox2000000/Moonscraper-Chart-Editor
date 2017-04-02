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

    public struct IntVector2
    {
        public int x, y;
        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
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

public static class Texture2DExtension
{
    public static Texture2D Inverse(this Texture2D sourceTex)
    {
        Color32[] pix = sourceTex.GetPixels32();
        System.Array.Reverse(pix);
        Texture2D destTex = new Texture2D(sourceTex.width, sourceTex.height);
        destTex.SetPixels32(pix);
        destTex.Apply();
        return destTex;
    }

    public static Texture2D HorizontalFlip(this Texture2D sourceTex)
    {
        Color32[] pix = sourceTex.GetPixels32();
        Color32[] flipped_pix = new Color32[pix.Length];

        for (int i = 0; i < pix.Length; i += sourceTex.width)
        {
            // Reverse the pixels row by row
            for (int j = i; j < i + sourceTex.width; ++j)
            {
                flipped_pix[j] = pix[i + sourceTex.width - (j - i) - 1];
            }
        }

        Texture2D destTex = new Texture2D(sourceTex.width, sourceTex.height);
        //destTex.alphaIsTransparency = sourceTex.alphaIsTransparency;
        destTex.SetPixels32(flipped_pix);
        destTex.Apply();
        return destTex;
    }

    public static Texture2D VerticalFlip(this Texture2D sourceTex)
    {
        Color32[] pix = sourceTex.GetPixels32();
        Color32[] flipped_pix = new Color32[pix.Length];

        for (int j = 0; j < sourceTex.height; ++j)
        {
            for (int i = 0; i < sourceTex.width; ++i)
            {
                flipped_pix[j * sourceTex.width + i] = pix[(sourceTex.height - 1 - j) * sourceTex.width + i];
            }
        }

        Texture2D destTex = new Texture2D(sourceTex.width, sourceTex.height);
        destTex.SetPixels32(flipped_pix);
        destTex.Apply();
        return destTex;
    }
}

public static class ColorExtension
{
    public static string GetHex(this Color col)
    {
        string hex = string.Empty;
        hex += ((int)col.r).ToString("x2");
        hex += ((int)col.g).ToString("x2");
        hex += ((int)col.b).ToString("x2");
        hex += ((int)col.a).ToString("x2");
        return hex;
    }
}
