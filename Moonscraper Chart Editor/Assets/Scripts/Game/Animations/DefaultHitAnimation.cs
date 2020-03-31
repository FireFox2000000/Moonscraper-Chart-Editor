// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultHitAnimation : HitAnimation {
    float initZPos;
    const float START_Z_POS = -START_ANIM_HEIGHT;

    protected SpriteRenderer ren;
    public SpriteRenderer baseRen;

    string initBaseLayerName;
    int initBaseLayerPos;

    bool startRan = false;
    bool isPressed = false;

	// Use this for initialization
	protected void Start () {
        if (!startRan)
        {
            initZPos = transform.position.z;
            ren = GetComponent<SpriteRenderer>();

            initBaseLayerName = baseRen.sortingLayerName;
            initBaseLayerPos = baseRen.sortingOrder;

            startRan = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;
        if (position.z < initZPos && running)
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

        if (position.z > initZPos || ChartEditor.Instance.currentState != ChartEditor.State.Playing)
        {
            position.z = initZPos;
            running = false;
        }

        transform.position = position;
        if (!(isPressed || running))
            gameObject.SetActive(false);
    }

    void OnDisable ()
    {
        StopAnim();
    }

    public override void StopAnim()
    {
        if (!ren)
            Start();

        Vector3 position = transform.position;
        position.z = initZPos;
        transform.position = position;

        if (ren)
        {
            ren.sortingLayerName = "Sustains";
            baseRen.sortingLayerName = initBaseLayerName;
            baseRen.sortingOrder = initBaseLayerPos;
        }
    }

    public override void PlayOneShot()
    {
        gameObject.SetActive(true);
        Vector3 position = transform.position;
        position.z = START_Z_POS;
        transform.position = position;
        running = true;
    }

    public override void Press()
    {
        gameObject.SetActive(true);
        isPressed = true;
    }

    public override void Release()
    {
        isPressed = false;
    }
}
