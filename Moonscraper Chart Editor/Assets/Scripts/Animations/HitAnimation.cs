using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitAnimation : MonoBehaviour {
    float initZPos;
    const float START_Z_POS = -0.25f;
    const float SPEED = 3;

    public bool running = false;

    SpriteRenderer ren;
    public SpriteRenderer baseRen;

    string initBaseLayerName;
    int initBaseLayerPos;

	// Use this for initialization
	void Start () {
        initZPos = transform.position.z;
        ren = GetComponent<SpriteRenderer>();

        initBaseLayerName = baseRen.sortingLayerName;
        initBaseLayerPos = baseRen.sortingOrder;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;
        if (position.z != initZPos)
        {
            gameObject.SetActive(true);
            ren.sortingLayerName = "Highlights";

            baseRen.sortingOrder = ren.sortingOrder - 1;
            baseRen.sortingLayerName = "Highlights";
        }
        else
        {
            ren.sortingLayerName = "Sustains";
            baseRen.sortingLayerName = initBaseLayerName;
            baseRen.sortingOrder = initBaseLayerPos;

            running = false;
        }

        position.z += SPEED * Time.deltaTime;

        if (position.z > initZPos || Globals.applicationMode != Globals.ApplicationMode.Playing)
            position.z = initZPos;

        transform.position = position;
        
    }

    public void StopAnim()
    {
        Vector3 position = transform.position;
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
