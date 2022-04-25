// https://answers.unity.com/questions/855483/capture-a-screen-shot-scene-shot-with-alpha.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenshotFunctions
{
    public static Texture2D TakeScreenShot()
    {
        return Screenshot();
    }

    static Texture2D Screenshot()
    {
        int resWidth = 1920;// Screen.width;// Camera.main.pixelWidth;
        int resHeight = 1080;// Screen.height;// Camera.main.pixelHeight;
        Camera camera = Camera.main;
        var clearFlags = camera.clearFlags;
        camera.clearFlags = CameraClearFlags.Depth;

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        rt.antiAliasing = 8;

        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        GameObject.Destroy(rt);

        camera.clearFlags = clearFlags;

        return screenShot;
    }

    public static Texture2D SaveScreenshotToFile(string fileName)
    {
        Texture2D screenShot = Screenshot();
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fileName, bytes);
        return screenShot;
    }
}
