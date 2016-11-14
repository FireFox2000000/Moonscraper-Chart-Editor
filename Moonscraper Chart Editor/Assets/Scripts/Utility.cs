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
