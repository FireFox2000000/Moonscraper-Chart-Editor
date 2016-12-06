using UnityEngine;
using System.Collections;

public class MeshSwapTest : MonoBehaviour {
    public Renderer[] renderers;
    public MeshFilter[] meshes;

    Renderer currentRenderer;
    MeshFilter currentMesh;

	// Use this for initialization
	void Start () {
        currentRenderer = GetComponent<Renderer>();
        currentMesh = GetComponent<MeshFilter>();
    }
	
	// Update is called once per frame
	void Update () {
        
	    if (Input.GetKeyDown("h"))
        {
            currentRenderer.materials = renderers[0].materials;
            currentMesh.mesh = meshes[0].mesh;
        }
        else if (Input.GetKeyDown("j"))
        {
            currentRenderer.materials = renderers[1].materials;
            currentMesh.mesh = meshes[1].mesh;
        }
        else if (Input.GetKeyDown("k"))
        {
            currentRenderer.materials = renderers[2].materials;
            currentMesh.mesh = meshes[2].mesh;
        }
    }
}
