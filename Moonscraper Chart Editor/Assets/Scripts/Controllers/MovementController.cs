using UnityEngine;
using System.Collections;

public abstract class MovementController : MonoBehaviour {
    public ChartEditor editor;

    protected Vector3 initPos;
    protected float scrollDelta = 0;

    bool focused = true;

    // Program options
    protected float mouseScrollSensitivity = 0.5f;

    // Jump to a chart position
    public abstract void SetPosition(uint chartPosition);

    protected void Start()
    {
        initPos = transform.position;
    }

    public void PlayingMovement()
    {
        // Auto scroll camera- Run in FixedUpdate
        float speed = Globals.hyperspeed;
        Vector3 pos = transform.position;
        pos.y += (speed * Time.deltaTime);
        transform.position = pos;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        focused = hasFocus;
    }

    void OnGUI()
    {
        if (focused)
        {
            if (UnityEngine.Event.current.type == EventType.ScrollWheel)
            {
                scrollDelta = -UnityEngine.Event.current.delta.y;
            }
            else
            {
                scrollDelta = 0;
            }
        }
    }
}
