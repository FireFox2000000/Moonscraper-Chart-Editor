using UnityEngine;
using System.Collections;

public class MouseDrag : MonoBehaviour {
    bool dragging;

    GameObject selectedGameObject;
	
	// Update is called once per frame
	void Update () {
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
            Draggable[] monos = selectedGameObject.GetComponents<Draggable>();
            foreach(Draggable mono in monos)
            {
                mono.OnRightMouseDrag();
            }
        }
    }
}

public class Draggable : MonoBehaviour
{
    public virtual void OnRightMouseDrag() { }
}
