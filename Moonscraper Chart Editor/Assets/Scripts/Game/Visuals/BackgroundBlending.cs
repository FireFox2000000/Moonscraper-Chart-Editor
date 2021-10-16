// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlending : MonoBehaviour
{
    public float bleadSpeed;

    Renderer ren;
    float delayTimer = 0;
    int currentBackground = 0;
    bool fadeRunning = false;

    Texture2D[] backgrounds;
    Texture initBGTex;

    // Use this for initialization
    void Start()
    {
        ren = GetComponent<Renderer>();

        // instanciate a copy of the material and edit that, otherwise Unity will update the timestamp of the original material asset without changes, which is very annoying for git chages detection. 
        ren.sharedMaterial = ren.material;

        initBGTex = ren.sharedMaterial.mainTexture;
        LoadBackgrounds();
    }

    void LoadBackgrounds()
    {
        backgrounds = GetAllBackgrounds();

        if (backgrounds.Length < 2)
        {
            enabled = false;
            Debug.LogWarning("At least 2 textures must be provided for background blending to work");
        }
        
        if (backgrounds.Length > 0)
        {
            ren.sharedMaterial.mainTexture = backgrounds[0];
        }
    }

    Texture2D[] GetAllBackgrounds()
    {
        List<Texture2D> customBackgrounds = new List<Texture2D>();
        int index = 0;
        while (true)
        {
            Texture2D tex = SkinManager.Instance.GetSkinItem<Texture2D>(SkinKeys.backgroundX + index++, null);
            if (!tex)
                break;
            customBackgrounds.Add(tex);
        }

        return customBackgrounds.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (!fadeRunning)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer > Globals.gameSettings.customBgSwapTime)
            {
                // Need to check if we need to wrap around to the start of the array.
                int nextBackground = currentBackground + 1;
                if (nextBackground >= backgrounds.Length)
                    nextBackground = 0;

                StartCoroutine(Fade(backgrounds[currentBackground], backgrounds[nextBackground]));
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
#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        Destroy(ren.material);
    }
#endif
}