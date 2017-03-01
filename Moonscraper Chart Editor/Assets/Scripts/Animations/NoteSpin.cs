using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpin : MonoBehaviour {
    const float SPIN_SPEED = -90;
    static float initTime = -1;
    static Vector3 initEular;

	// Use this for initialization
	void Start () {
        if (initTime == -1)
        {
            initTime = Time.realtimeSinceStartup;
            initEular = transform.rotation.eulerAngles;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Euler(new Vector3(SPIN_SPEED * (Time.realtimeSinceStartup - initTime), initEular.y, initEular.z));
	}
}
