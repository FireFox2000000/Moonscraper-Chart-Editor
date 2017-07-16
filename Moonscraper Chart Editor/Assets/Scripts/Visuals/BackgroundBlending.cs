using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlending : MonoBehaviour
{
    public float bleadSpeed;
    public Skin skin;

    Renderer ren;
    float delayTimer = 0;
    int currentBackground = 0;
    bool fadeRunning = false;

    // Use this for initialization
    void Start()
    {
        ren = GetComponent<Renderer>();

        if (skin.backgrounds.Length < 2)
        {
            enabled = false;
            Debug.LogError("At least 2 textures must be provided for background blending to work");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!fadeRunning)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer > Globals.customBgSwapTime)
            {
                // Need to check if we need to wrap around to the start of the array.
                int nextBackground = currentBackground + 1;
                if (nextBackground >= skin.backgrounds.Length)
                    nextBackground = 0;

                StartCoroutine(Fade(skin.backgrounds[currentBackground], skin.backgrounds[nextBackground]));
                currentBackground = nextBackground;

                delayTimer = 0;
            }
        }
    }

    IEnumerator Fade(Texture2D startTex, Texture2D endTex)
    {
        fadeRunning = true;

        float t = 0;

        ren.sharedMaterial.mainTexture = startTex;
        ren.sharedMaterial.SetTexture("_BlendTex", endTex);

        while (t <= 1)
        {
            t += Time.deltaTime * bleadSpeed;

            ren.sharedMaterial.SetFloat("_Blend", t);

            yield return null;
        }

        ren.sharedMaterial.SetFloat("_Blend", 0);
        ren.sharedMaterial.mainTexture = endTex;

        fadeRunning = false;
    }

    void OnApplicationQuit()
    {
        ren.sharedMaterial.SetFloat("_Blend", 0);
    }
}