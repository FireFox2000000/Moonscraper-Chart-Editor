// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class FretboardWrapMovement : MonoBehaviour {
    Renderer ren;
    float prevYPos;
    float prevHyperspeed;

    // Use this for initialization
    void Start () {
        ren = GetComponent<Renderer>();
        prevYPos = transform.position.y;
        prevHyperspeed = Globals.gameSettings.hyperspeed * Globals.gameSettings.gameSpeed;
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void OnApplicationQuit()
    {
        // Reset purely for editor
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void LateUpdate()
    {
        if (ChartEditor.Instance.currentState == ChartEditor.State.Playing)
        {
            Vector2 offset = ren.sharedMaterial.mainTextureOffset;
            offset.y += ((transform.position.y - prevYPos) / transform.localScale.y);
            ren.sharedMaterial.mainTextureOffset = offset;

            prevYPos = transform.position.y;
            prevHyperspeed = Globals.gameSettings.hyperspeed * Globals.gameSettings.gameSpeed;
        }
    }
    
	// Update is called once per frame
	void FixedUpdate () {
        if (ChartEditor.Instance.currentState == ChartEditor.State.Editor)  
        {
            if ((int)(prevHyperspeed * 100) == (int)((Globals.gameSettings.hyperspeed * Globals.gameSettings.gameSpeed) * 100))
            {
                Vector2 offset = ren.sharedMaterial.mainTextureOffset;
                offset.y += (transform.position.y - prevYPos) / transform.localScale.y;
                ren.sharedMaterial.mainTextureOffset = offset;
            }

            prevYPos = transform.position.y;
            prevHyperspeed = Globals.gameSettings.hyperspeed * Globals.gameSettings.gameSpeed;
        } 
    }
}
