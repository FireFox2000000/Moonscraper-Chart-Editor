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
        prevHyperspeed = Globals.hyperspeed * Globals.gameSpeed;
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void OnApplicationQuit()
    {
        // Reset purely for editor
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void LateUpdate()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            Vector2 offset = ren.sharedMaterial.mainTextureOffset;
            offset.y += (transform.position.y - prevYPos) / transform.localScale.y;
            ren.sharedMaterial.mainTextureOffset = offset;

            prevYPos = transform.position.y;
            prevHyperspeed = Globals.hyperspeed * Globals.gameSpeed;
        }
    }
    
	// Update is called once per frame
	void FixedUpdate () {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)  
        {
            if (prevHyperspeed == Globals.hyperspeed * Globals.gameSpeed)
            {
                Vector2 offset = ren.sharedMaterial.mainTextureOffset;
                offset.y += (transform.position.y - prevYPos) / transform.localScale.y;
                ren.sharedMaterial.mainTextureOffset = offset;
            }
            prevYPos = transform.position.y;
            prevHyperspeed = Globals.hyperspeed * Globals.gameSpeed;
        } 
    }
}
