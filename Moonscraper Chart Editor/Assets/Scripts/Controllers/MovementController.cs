using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MovementController : MonoBehaviour {   
    public Scrollbar scrollBar;
    public RectTransform content;
    
    Vector3 initPos;
    float scrollDelta = 0;

    // Program options
    float mouseScrollSensitivity = 0.5f;

    public ChartEditor editor;

    public MovementMode movementMode = MovementMode.Editor;

    // Use this for initialization
    void Start () {
        initPos = transform.position;
        scrollBar.value = 0;
        UpdatePosBasedScrollValue();       
    }

    Vector3 prevPos = Vector3.zero;

    // Update is called once per frame
	void Update () {
        if (movementMode == MovementMode.Editor)
        {
            if (scrollDelta == 0)
            {
                scrollDelta = Input.mouseScrollDelta.y;
            }

            // Position changes scroll bar value
            if (scrollDelta != 0 || transform.position != prevPos)
            {
                // Mouse scroll movement
                transform.position = new Vector3(transform.position.x, transform.position.y + (scrollDelta * mouseScrollSensitivity), transform.position.z);

                if (transform.position.y < initPos.y)
                    transform.position = initPos;

                UpdateScrollValueBasedPos();
            }

            // Scroll bar value changes position
            else
            {
                UpdatePosBasedScrollValue();
            }
        }
        else if (movementMode == MovementMode.Playing)
        {
            // Auto scroll camera
            float speed = Globals.hyperspeed;
            Vector3 pos = transform.position;
            pos.y += (speed * Time.deltaTime);
            transform.position = pos;

            UpdateScrollValueBasedPos();
        }

        prevPos = transform.position;
    }

    public void SetPosition(int chartPosition)
    {
        Vector3 pos = initPos;
        pos.y += editor.currentSong.ChartPositionToWorldYPosition(chartPosition);
        transform.position = pos;
    }

    public IEnumerator ResetPosition()
    {
        transform.position = initPos;

        yield return null;
        scrollBar.value = 0;
    }

    void UpdateScrollValueBasedPos()
    {
        UpdateContentHeight();

        // Update the scroll value
        scrollBar.value = (transform.position.y - initPos.y) / (content.sizeDelta.y * content.transform.lossyScale.y);
    }

    void UpdatePosBasedScrollValue()
    {       
        // Grabbing the scrollbar
        float pos = content.rect.height * content.lossyScale.y * scrollBar.value;

        // Apply the position
        transform.position = new Vector3(transform.position.x, pos + initPos.y, transform.position.z);

        UpdateContentHeight();
    }

    void UpdateContentHeight()
    {
        // Update the content height
        float user_pos = transform.position.y + Camera.main.orthographicSize - initPos.y;
        float max = user_pos;
        if (editor.currentChart != null && editor.currentChart.Length > 0)
        {
            float posOfFinalNote = editor.currentSong.ChartPositionToWorldYPosition(editor.currentChart[editor.currentChart.Length - 1].position);

            if (editor.currentChart.Length > 0 && posOfFinalNote > user_pos)
                max = posOfFinalNote;

            ContentHeight(content, max);
        }
    }

    void ContentHeight(RectTransform content, float maxHeight)
    {
        const float MINHEIGHT = 300;
        float height = maxHeight / content.transform.lossyScale.y;
        if (height < MINHEIGHT)
            height = MINHEIGHT;
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }

    void OnGUI()
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

    public enum MovementMode
    {
        Editor, Playing
    }
}
