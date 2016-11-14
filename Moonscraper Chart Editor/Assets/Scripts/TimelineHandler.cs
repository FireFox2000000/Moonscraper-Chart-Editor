using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class TimelineHandler : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    GameObject handle;
    public UnityEngine.UI.Text percentage;

    RectTransform rectTransform;
    MovementController movement;

    float halfHeight;
    float scaledHalfHeight;

    // Value between 0 and 1
    public float handlePos
    {
        get
        {
            return (handle.transform.localPosition.y + halfHeight) / rectTransform.sizeDelta.y;
        }
        set
        {
            if (value > 1)
                value = 1;
            else if (value < 0)
                value = 0;

            Vector3 pos = handle.transform.localPosition;
            pos.y = value * rectTransform.sizeDelta.y - halfHeight;
            handle.transform.localPosition = pos;
        }
    }                    

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        halfHeight = rectTransform.sizeDelta.y / 2.0f;
        scaledHalfHeight = halfHeight * transform.lossyScale.y;

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
    }

    void Update()
    {
        halfHeight = rectTransform.sizeDelta.y / 2.0f;
        scaledHalfHeight = halfHeight * transform.lossyScale.y;

        percentage.text = ((int)(handlePos * 100)).ToString() + "%";

        //Debug.Log(handlePos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        moveHandle(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        moveHandle(eventData);
    }

    void moveHandle(PointerEventData eventData)
    {
        if (movement.applicationMode == MovementController.ApplicationMode.Editor)
        {
            Vector3 pos = handle.transform.position;
            pos.y = eventData.position.y;

            if (pos.y > transform.position.y + scaledHalfHeight)
                pos.y = transform.position.y + scaledHalfHeight;
            else if (pos.y < transform.position.y - scaledHalfHeight)
                pos.y = transform.position.y - scaledHalfHeight;

            handle.transform.position = pos;
        }
    }
}
