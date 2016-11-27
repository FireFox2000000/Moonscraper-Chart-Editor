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
        prevHyperspeed = Globals.hyperspeed;
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
            prevHyperspeed = Globals.hyperspeed;
        }
    }
    
	// Update is called once per frame
	void FixedUpdate () {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)  
        {
            if (prevHyperspeed == Globals.hyperspeed)
            {
                Vector2 offset = ren.sharedMaterial.mainTextureOffset;
                Debug.Log(offset);
                offset.y += (transform.position.y - prevYPos) / transform.localScale.y;
                Debug.Log((transform.position.y - prevYPos) / transform.localScale.y);
                Debug.Log(offset);
                ren.sharedMaterial.mainTextureOffset = offset;
            }
            prevYPos = transform.position.y;
            prevHyperspeed = Globals.hyperspeed;
        } 
    }
}
