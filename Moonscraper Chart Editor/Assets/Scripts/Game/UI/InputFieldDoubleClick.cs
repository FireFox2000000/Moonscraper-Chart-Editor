// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

// This script is automatically distributed to all input fields via the UiServices script
[RequireComponent(typeof(InputField))]
public class InputFieldDoubleClick : MonoBehaviour { 
    InputField input;
    float lastClickTime;
    const float DOUBLE_CLICK_TIME = 0.3f;

    void Start()
    {
        input = GetComponent<InputField>();
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { Click(); });
        eventTrigger.triggers.Add(entry);

        lastClickTime = Time.realtimeSinceStartup - DOUBLE_CLICK_TIME;
    }

    void Click()
    {
        if (Time.realtimeSinceStartup - lastClickTime < DOUBLE_CLICK_TIME)
            HighlightInputField();

        lastClickTime = Time.realtimeSinceStartup;
    }

    void HighlightInputField()
    {
        input.DeactivateInputField();
        input.ActivateInputField();
    }
}
