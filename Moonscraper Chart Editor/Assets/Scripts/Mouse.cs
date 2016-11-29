using UnityEngine;
using System.Collections;

public class Mouse : MonoBehaviour {
    [Header("Viewing modes")]
    public Camera camera2D;
    public Camera camera3D;

    bool dragging;

    ChartEditor editor;
    GameObject selectedGameObject;
	
    public static Vector2? world2DPosition = null;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

	// Update is called once per frame
	void Update () {
        // Calculate world2DPosition
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        RaycastHit[] planeHit;
        planeHit = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

        if (planeHit.Length > 0)
            world2DPosition = planeHit[0].point;
        else
            world2DPosition = null;

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && world2DPosition != null)
        {
            dragging = true;

            selectedGameObject = GetSongObjectUnderMouse();
            /*
            if (Toolpane.currentTool == Toolpane.Tools.Cursor && selectedGameObject == null)
                editor.currentSelectedObject = null;*/
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            dragging = false;

            selectedGameObject = null;
        }

        // OnMouseDrag
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

    static RaycastHit2D lowestY(RaycastHit2D[] hits)
    {
        RaycastHit2D lowestHit = hits[0];

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.transform.position.y < lowestHit.collider.gameObject.transform.position.y)
                lowestHit = hit;
        }

        return lowestHit;
    }

    public static GameObject GetSongObjectUnderMouse()
    {
        if (world2DPosition != null)
        {
            LayerMask mask = 1 << LayerMask.NameToLayer("SongObject");
            RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)world2DPosition, Vector2.zero, 0, mask);

            if (hits.Length > 0)
            {
                RaycastHit2D lowestYHit = lowestY(hits);

                if (lowestYHit.collider)
                    return lowestYHit.collider.gameObject;
            }
        }

        return null;
    }
}

public class Draggable : MonoBehaviour
{
    public virtual void OnRightMouseDrag() { }
}
