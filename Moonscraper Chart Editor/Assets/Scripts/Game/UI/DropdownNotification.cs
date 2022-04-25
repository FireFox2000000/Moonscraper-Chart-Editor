// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownNotification : MonoBehaviour {
    [SerializeField]
    float dropdownTime = 0.5f;
    [SerializeField]
    float dropdownDistance = 1.0f;
    [SerializeField]
    UnityEngine.UI.Text notificationText;

    class NotificationData
    {
        public string message;
        public float displayTime;
        public bool cancelable;

        public NotificationData(string message, float displayTime, bool cancelable)
        {
            this.message = message;
            this.displayTime = displayTime;
            this.cancelable = cancelable;
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
    float defaultAspectRatio = 16.0f / 9.0f;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.localPosition.y;
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (currentState == State.Open)
        {
            if (currentNotification != null && (notificationTimer > currentNotification.displayTime || (currentNotification.cancelable && notificationQueue.Count > 0)))
            {
                Close();
            }

            notificationTimer += Time.deltaTime;
        }
        else
        {
            notificationTimer = 0;
        }

        var currentResolution = Screen.currentResolution;
        float currentAspectRatio = currentResolution.width / currentResolution.height;
        float scaledDropDistance = dropdownDistance * defaultAspectRatio / currentAspectRatio;

        float transitionDistance = scaledDropDistance / dropdownTime * Time.deltaTime;
        if (currentState == State.Closed && notificationQueue.Count > 0)
        {
            currentNotification = PopNotification();
            Open(currentNotification);
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

                // Disable
                if (notificationQueue.Count <= 0)
                    gameObject.SetActive(false);
            }

            rectTransform.localPosition = position;
        }
	}

    void Open(NotificationData notificationData)
    {
        notificationText.text = notificationData.message;

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

    public void PushNotification(string message, float displayTime = 3, bool cancelable = false)
    {
        notificationQueue.Add(new NotificationData(message, displayTime, cancelable));
        gameObject.SetActive(true);
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
