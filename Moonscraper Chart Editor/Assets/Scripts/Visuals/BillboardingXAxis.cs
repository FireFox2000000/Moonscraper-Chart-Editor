using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BillboardingXAxis : MonoBehaviour {

	void Start () { 
        Vector3 eular = transform.eulerAngles;
        transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, eular.y, eular.z);
        enabled = false;
	}
}
