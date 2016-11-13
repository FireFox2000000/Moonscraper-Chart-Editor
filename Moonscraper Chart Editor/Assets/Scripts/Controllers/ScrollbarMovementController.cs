using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollbarMovementController : MovementController {   
    public Scrollbar scrollBar;
    public RectTransform content;
    
    Vector3 initPos;
    float scrollDelta = 0;

    // Program options
    float mouseScrollSensitivity = 0.5f;

    public ChartEditor editor;

    // Use this for initialization
    void Start () {
        initPos = transform.position;
        scrollBar.value = 0;
        UpdatePosBasedScrollValue();       
    }

    Vector3 prevPos = Vector3.zero;

    // Update is called once per frame
	void FixedUpdate () {
        if (applicationMode == ApplicationMode.Editor)
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
        else if (applicationMode == ApplicationMode.Playing)
        {
            PlayingMovement();

            UpdateScrollValueBasedPos();
        }

        prevPos = transform.position;
    }

    public override void SetPosition(uint chartPosition)
    {
        Vector3 pos = initPos;
        pos.y += editor.currentSong.ChartPositionToWorldYPosition(chartPosition);
        transform.position = pos;
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
        if (editor.currentChart != null && editor.currentChart.notes.Length > 0)
        {
            float posOfFinalNote = editor.currentChart.notes[editor.currentChart.notes.Length - 1].worldYPosition;
            //max = Song.TimeToWorldYPosition(editor.currentChart.endTime);
            if (editor.currentChart.notes.Length > 0 && posOfFinalNote > user_pos)
                max = posOfFinalNote;

            
        }
        ContentHeight(content, max);
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
}
