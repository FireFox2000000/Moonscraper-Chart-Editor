using UnityEngine;
using System.Collections;

public class StarpowerTailController : MonoBehaviour {
    public StarpowerController spCon;

    void OnMouseDrag()
    {
        // Update sustain
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            spCon.TailDrag();
        }
    }
}
