using UnityEngine;
using System.Collections;

public class MeshSwapTest : MonoBehaviour {

    public Mesh[] meshes;

    MeshFilter currentMesh;

	// Use this for initialization
	void Start () {
        currentMesh = GetComponent<MeshFilter>();
    }
	
	// Update is called once per frame
	void Update () {
        /*
	    if (Input.GetKeyDown("h"))
        {
            currentMesh.mesh = meshes[0];
        }
        else if (Input.GetKeyDown("j"))
        {
            currentMesh.mesh = meshes[1];
        }
        else if (Input.GetKeyDown("k"))
        {
            currentMesh.mesh = meshes[2];
        }*/
    }
}
