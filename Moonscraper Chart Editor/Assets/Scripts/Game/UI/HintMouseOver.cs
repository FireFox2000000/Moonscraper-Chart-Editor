// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HintMouseOver : MonoBehaviour {
    const float WAIT_TIME = 0.25f;
    const float MAX_TEXTBOX_WIDTH = 150;

    // Used for GUI matrix scaling
    const float NATIVE_WIDTH = 1920.0f;
    const float nativeHeight = 1080.0f;

    public string message;
    string localisedMessage;
    public static GUIStyle style;
    public Vector2 offset;

    Vector2? lastMousePos = null;
    float timer;

    bool active = false;
    bool exiting = false;

    float alpha = 0;

    void Start()
    {
        OnLocalise();

        // Generate event triggers for when script is placed on UI
        var trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;

        entry.callback.AddListener((eventData) => { OnMouseEnter(); });
        exit.callback.AddListener((eventData) => { OnMouseExit(); });

        trigger.triggers.Add(entry);
        trigger.triggers.Add(exit);

        enabled = false;
    }

	void Update()
    {
        if (exiting)
            return;

        if (lastMousePos == Input.mousePosition)
            timer += Time.deltaTime;
        else
            timer = 0;

        if (timer > WAIT_TIME)
        {
            // Generate hint overlay
            if (!active)
            {
                GenerateHintBox();
            }
        }

        if (Input.GetMouseButtonDown(0))
            OnMouseExit();

        lastMousePos = Input.mousePosition;
    }

    void OnMouseEnter()
    {
        enabled = true;
        exiting = false;
        alpha = 0;
    }

    void OnMouseExit()
    {
        lastMousePos = null;
        timer = 0;

        DestroyHintBox();
    }

    void OnGUI()
    {
        if (active && alpha > 0)
        {
            Vector2 scaledOffset = new Vector2(offset.x * transform.lossyScale.x, offset.y * transform.lossyScale.y);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(ChartEditor.Instance.uiServices.uiCamera, ((Vector2)transform.position + scaledOffset));
            screenPos.y = Screen.height - screenPos.y;      // Alter for GUI label position

            // Apply matrix transformations
            const float NATIVE_WIDTH = 1920.0f;
            const float NATIVE_HEIGHT = 1080.0f;
            float rx = Screen.width / NATIVE_WIDTH;
            float ry = Screen.height / NATIVE_HEIGHT;
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(rx, ry, 1));

            // Apply fading
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);

            // Calculate the size of the textbox
            Vector2 size = style.CalcSize(new GUIContent(localisedMessage));
            size.x += 1;        // Some weird Unity 2018 thing is cutting text off by a single pixel. Thanks Unity. 
            if (size.x > MAX_TEXTBOX_WIDTH)
            {
                size.x = MAX_TEXTBOX_WIDTH;
                size.y = style.CalcHeight(new GUIContent(localisedMessage), MAX_TEXTBOX_WIDTH);
            }

            // Check if the box is going to appear offscreen and fix
            if (screenPos.x < 0)
                screenPos.x = 0;

            if (screenPos.x + size.x * rx > Screen.width)
                screenPos.x = Screen.width - size.x * rx;

            if (screenPos.y < 0)
                screenPos.y = 0;

            if (screenPos.y + size.y * ry > Screen.height)
                screenPos.y = Screen.height - size.y * ry;

            GUI.Label(new Rect(screenPos.x / rx, screenPos.y / ry, size.x, size.y), localisedMessage, style);
        }
    }

    void GenerateHintBox()
    {
        active = true;

        StartCoroutine(FadeIn());
    }

    void DestroyHintBox()
    {
        exiting = true;
        StartCoroutine(FadeOut());
    }

    const float FADE_SPEED = 10.0f;

    bool cancel = false;
    IEnumerator FadeIn()
    {
        alpha = 0;

        cancel = true;
        yield return null;
        cancel = false;

        while (alpha < 1 && !cancel)
        {
            alpha += FADE_SPEED * Time.deltaTime;

            yield return null;
        }

        alpha = 1;
    }

    IEnumerator FadeOut()
    {
        alpha = 1;

        cancel = true;
        yield return null;
        cancel = false;

        while (alpha > 0 && !cancel)
        {
            alpha -= FADE_SPEED * Time.deltaTime;

            yield return null;
        }

        alpha = 0;
        active = false;
        enabled = false;
    }

    void OnLocalise()
    {
        localisedMessage = Localiser.Instance.Localise(message);
    }
}
