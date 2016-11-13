using UnityEngine;
using System.Collections;

public abstract class MovementController : MonoBehaviour {
    [HideInInspector]
    public ApplicationMode applicationMode = ApplicationMode.Editor;

    // Jump to a chart position
    public abstract void SetPosition(uint chartPosition);

    public void PlayingMovement()
    {
        // Auto scroll camera- Run in FixedUpdate
        float speed = Globals.hyperspeed;
        Vector3 pos = transform.position;
        pos.y += (speed * Time.fixedDeltaTime);
        transform.position = pos;
    }

    public enum ApplicationMode
    {
        Editor, Playing
    }
}
