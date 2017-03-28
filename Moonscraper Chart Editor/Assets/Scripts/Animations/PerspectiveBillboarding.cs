using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveBillboarding : MonoBehaviour {
    const float ROTATION_FACTOR = 15;

    Vector3 initRotation;

	// Use this for initialization
	void Start () {
        initRotation = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
        float screenPosY = Camera.main.WorldToScreenPoint(transform.position).y;
        float percentageofScreenHeight = screenPosY / Screen.height;

        Vector3 rotation = transform.rotation.eulerAngles;  

        if (Camera.main.orthographic)
            rotation.x = initRotation.x;
        else
            rotation.x = initRotation.x - (percentageofScreenHeight * 2 - 1) * ROTATION_FACTOR;

        transform.rotation = Quaternion.Euler(new Vector3(rotation.x, 0, 0));
    }
}
