using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class TimelineHandler : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    GameObject handle;
    public UnityEngine.UI.Text percentage;
    public GameObject sectionIndicatorPrefab;

    GameObject indicators;
    SectionGuiController[] sectionIndicatorPool = new SectionGuiController[100];

    RectTransform rectTransform;
    MovementController movement;

    float halfHeight;
    float scaledHalfHeight;

    ChartEditor editor;

    // Value between 0 and 1
    public float handlePosRound
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

    public float handlePos
    {
        get
        {
            return (handle.transform.localPosition.y + halfHeight) / rectTransform.sizeDelta.y;
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

    void Start()
    {
        indicators = new GameObject("Indicators");
        indicators.transform.SetParent(this.transform.parent);
        indicators.transform.localPosition = Vector3.zero;
        indicators.transform.localScale = new Vector3(1, 1, 1);
        indicators.transform.SetSiblingIndex(1);

        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        for (int i = 0; i < sectionIndicatorPool.Length; ++i)
        {
            GameObject sectionIndicator = Instantiate(sectionIndicatorPrefab);
            sectionIndicator.gameObject.transform.SetParent(indicators.transform);

            sectionIndicatorPool[i] = sectionIndicator.GetComponent<SectionGuiController>();
            sectionIndicatorPool[i].timelineHandler = this;
            sectionIndicatorPool[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        halfHeight = rectTransform.sizeDelta.y / 2.0f;
        scaledHalfHeight = halfHeight * transform.lossyScale.y;

        percentage.text = ((int)(handlePosRound * 100)).ToString() + "%";

        // Set the sections
        for (int i = 0; i < editor.currentSong.sections.Length; ++i)
        {
            if (i < sectionIndicatorPool.Length)
            {
                sectionIndicatorPool[i].Init(editor.currentSong.sections[i]);
                sectionIndicatorPool[i].gameObject.SetActive(true);
            }
            else
            {
                while (i < sectionIndicatorPool.Length)
                {
                    sectionIndicatorPool[i++].gameObject.SetActive(false);
                }
                break;
            }
        }
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
        movement.editor.Stop();
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
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
        // Audio may be shorter than the chart
        /*if (pos > 1)
            pos = 1;
        else */if (pos < 0)
            pos = 0;
        return new Vector3(handle.transform.localPosition.x, pos * rectTransform.sizeDelta.y - halfHeight, handle.transform.localPosition.z);
    }
}
