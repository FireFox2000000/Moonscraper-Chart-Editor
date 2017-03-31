using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSpriteTest : MonoBehaviour {
    public SpriteNoteResources resources;

    SpriteRenderer ren;
	// Use this for initialization
	void Start () {
        ren = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        ren.sprite = resources.reg_strum[0];
	}
}
