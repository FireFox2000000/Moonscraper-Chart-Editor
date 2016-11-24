using UnityEngine;
using System.Collections;

public class Mouse : MonoBehaviour {
    [Header("Viewing modes")]
    public Camera camera2D;
    public Camera camera3D;

    bool dragging;

    GameObject selectedGameObject;
	
    public static Vector2? world2DPosition = null;

	// Update is called once per frame
	void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        RaycastHit[] planeHit;
        planeHit = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

        if (planeHit.Length > 0)
            world2DPosition = planeHit[0].point;
        else
            world2DPosition = null;

        if (Input.GetMouseButtonDown(1))
        {
            dragging = true;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider)
            {
                selectedGameObject = hit.collider.gameObject;
            }

        }
        if (Input.GetMouseButtonUp(1))
        {
            dragging = false;

            selectedGameObject = null;
        }

        if (dragging && selectedGameObject)
        {
            MonoBehaviour[] monos = selectedGameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mono in monos)
            {
                mono.SendMessage("OnMouseDrag");
            }
        }
    }

    public void SwitchCamera()
    {
        if (camera2D.gameObject.activeSelf)
        {
            camera2D.gameObject.SetActive(false);
            camera3D.gameObject.SetActive(true);
        }
        else
        {
            camera3D.gameObject.SetActive(false);
            camera2D.gameObject.SetActive(true);
        }
    }
}

public class Draggable : MonoBehaviour
{
    public virtual void OnRightMouseDrag() { }
}
