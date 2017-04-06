using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BillboardingXAxis : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        Vector3 eular = transform.eulerAngles;
        transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, eular.y, eular.z);
	}
}
