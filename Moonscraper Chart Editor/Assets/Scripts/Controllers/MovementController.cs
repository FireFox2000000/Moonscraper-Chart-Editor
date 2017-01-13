using UnityEngine;
using System.Collections;

public abstract class MovementController : MonoBehaviour {
    public static bool cancel = false;
    public ChartEditor editor;
    protected Globals globals;

    public Vector3 initPos { get; protected set; }
    protected float scrollDelta = 0;

    protected bool focused = true;

    // Program options
    protected float mouseScrollSensitivity = 0.2f;      // May miss snap gaps if placed too high

    // Jump to a chart position
    public abstract void SetPosition(uint chartPosition);

    protected void Start()
    {
        initPos = transform.position;
        globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<Globals>();
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
        else
            scrollDelta = 0;
    }
}
