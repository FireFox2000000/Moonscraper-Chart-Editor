using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScreenshot : MonoBehaviour
{
    [SerializeField]
    GameObject[] objectsToDisableDuringNotesOnlyScreenshot = new GameObject[0];

    public void TakeScreenshot()
    {
        string filename;
        FileExplorer.SaveFilePanel(new ExtensionFilter("Images files", "png"), "screenshot", "png", out filename);

        ScreenshotFunctions.SaveScreenshotToFile(filename);
    }

    public void SceenCaptureNotesOnly()
    {
        foreach (GameObject go in objectsToDisableDuringNotesOnlyScreenshot)
        {
            go.SetActive(false);
        }

        GameObject beatLines = GameObject.Find("Beat Lines");
        beatLines.SetActive(false);

        MKGlowSystem.MKGlow mkGlow = GameObject.FindObjectOfType<MKGlowSystem.MKGlow>();
        mkGlow.enabled = false;

        TakeScreenshot();

        foreach (GameObject go in objectsToDisableDuringNotesOnlyScreenshot)
        {
            go.SetActive(true);
        }

        beatLines.SetActive(true);
        mkGlow.enabled = true;
    }
}
