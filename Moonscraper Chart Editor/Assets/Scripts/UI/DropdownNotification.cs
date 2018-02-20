using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownNotification : MonoBehaviour {
    [SerializeField]
    float dropdownTime = 0.5f;
    [SerializeField]
    float dropdownDistance = 1.0f;

    class NotificationData
    {
        public string message;
        public float displayTime;

        public NotificationData(string message, float displayTime)
        {
            this.message = message;
            this.displayTime = displayTime;
        }
    }

    enum State
    {
        Closed,
        Opening,
        Open,
        Closing,
    }

    List<NotificationData> notificationQueue = new List<NotificationData>();
    NotificationData currentNotification;
    State currentState = State.Closed;
    float notificationTimer = 0;
    float originalPosition;
    RectTransform rectTransform;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
        if (currentState == State.Open)
        {
            if (currentNotification != null && notificationTimer > currentNotification.displayTime)
            {
                Close();
            }

            notificationTimer += Time.deltaTime;
        }
        else
        {
            notificationTimer = 0;
        }

        float transitionDistance = dropdownDistance / dropdownTime * Time.deltaTime;
        if (currentState == State.Closed && notificationQueue.Count > 0)
        {
            currentNotification = PopNotification();
            Open();
        }
        else if (currentState == State.Opening)
        {
            Vector3 position = rectTransform.localPosition;
            position.y -= transitionDistance;
            float finalPosition = originalPosition - dropdownDistance;

            if (position.y <= finalPosition)
            {
                position.y = finalPosition;
                currentState = State.Open;
            }

            rectTransform.localPosition = position;
        }
        else if (currentState == State.Closing)
        {
            Vector3 position = rectTransform.localPosition;
            position.y += transitionDistance;

            if (position.y >= originalPosition)
            {
                position.y = originalPosition;
                currentState = State.Closed;
            }

            rectTransform.localPosition = position;
        }
	}

    void Open()
    {
        if (currentState == State.Closed)
            currentState = State.Opening;
    }

    void Close()
    {
        if (currentState == State.Open)
        {
            currentState = State.Closing;
            currentNotification = null;
        }
    }

    public void PushNotification(string message, float displayTime)
    {
        notificationQueue.Add(new NotificationData(message, displayTime));
    }

    NotificationData PopNotification()
    {
        NotificationData notification = null;

        if (notificationQueue.Count > 0)
        {
            notification = notificationQueue[0];
            notificationQueue.RemoveAt(0);
        }

        return notification;
    }
}
