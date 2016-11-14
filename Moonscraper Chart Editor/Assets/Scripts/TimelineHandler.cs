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
            return (handle.transform.localPosition.y.Round(2) + halfHeight.Round(2)) / rectTransform.sizeDelta.y.Round(2);
        }
        set
        {
            handle.transform.localPosition = handlePosToLocal(value);
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

    public Vector3 handlePosToLocal(float pos)
    {
        if (pos > 1)
            pos = 1;
        else if (pos < 0)
            pos = 0;
        return new Vector3(handle.transform.localPosition.x, pos * rectTransform.sizeDelta.y - halfHeight, handle.transform.localPosition.z);
    }
}
