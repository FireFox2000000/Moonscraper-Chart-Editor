using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HintMouseOver : MonoBehaviour {
    const float WAIT_TIME = 0.25f;
    const float MAX_TEXTBOX_WIDTH = 100;

    // Used for GUI matrix scaling
    const float NATIVE_WIDTH = 1920.0f;
    const float nativeHeight = 1080.0f;

    public string message;
    public GUIStyle style;
    public Vector2 offset;

    Vector2? lastMousePos = null;
    float timer;

    bool active = false;
    bool exiting = false;

    float alpha = 0;

    void Start()
    {
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

        if (active)
            DestroyHintBox();
    }

    void OnGUI()
    {
        if (active && alpha > 0)
        {
            Vector2 scaledOffset = new Vector2(offset.x * transform.lossyScale.x, offset.y * transform.lossyScale.y);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, ((Vector2)transform.position + scaledOffset));

            const float NATIVE_WIDTH = 1920.0f;
            const float NATIVE_HEIGHT = 1080.0f;
            float rx = Screen.width / NATIVE_WIDTH;
            float ry = Screen.height / NATIVE_HEIGHT;
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(rx, ry, 1));

            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);

            Vector2 size = style.CalcSize(new GUIContent(message));
            if (size.x > MAX_TEXTBOX_WIDTH)
            {
                size.x = MAX_TEXTBOX_WIDTH;
                size.y = style.CalcHeight(new GUIContent(message), MAX_TEXTBOX_WIDTH);
            }

            GUI.Label(new Rect(screenPos.x / rx, (Screen.height - screenPos.y) / ry, size.x, size.y), message, style);
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

    IEnumerator FadeIn()
    {
        const float SPEED = 10.0f;

        alpha = 0;

        while (alpha < 1)
        {
            alpha += SPEED * Time.deltaTime;

            yield return null;
        }

        alpha = 1;
    }

    IEnumerator FadeOut()
    {
        const float SPEED = 10.0f;

        alpha = 1;

        while (alpha > 0)
        {
            alpha -= SPEED * Time.deltaTime;

            yield return null;
        }

        alpha = 0;
        active = false;
        enabled = false;
    }
}
