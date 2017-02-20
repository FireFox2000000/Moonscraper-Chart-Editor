using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitAnimation : MonoBehaviour {
    float initZPos;
    const float START_Z_POS = -0.25f;
    const float SPEED = 3;

    public bool running = false;

    SpriteRenderer ren;

	// Use this for initialization
	void Start () {
        initZPos = transform.position.z;
        ren = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;
        if (position.z != initZPos)
        {
            gameObject.SetActive(true);
            ren.sortingLayerName = "Highlights";
        }
        else
        {
            ren.sortingLayerName = "Sustains";
            running = false;
        }

        position.z += SPEED * Time.deltaTime;

        if (position.z > initZPos)
            position.z = initZPos;

        transform.position = position;
        
    }

    public void PlayOneShot()
    {
        gameObject.SetActive(true);
        Vector3 position = transform.position;
        position.z = START_Z_POS;
        transform.position = position;
        running = true;      
    }
}
