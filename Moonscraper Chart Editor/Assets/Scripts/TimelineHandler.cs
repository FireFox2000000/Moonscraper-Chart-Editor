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
    public GameObject starpowerIndicatorPrefab;

    const int POOL_SIZE = 100;
    GameObject sectionIndicatorParent;
    SectionGuiController[] sectionIndicatorPool = new SectionGuiController[POOL_SIZE];
    GameObject starpowerIndicatorParent;
    StarpowerGUIController[] starpowerIndicatorPool = new StarpowerGUIController[POOL_SIZE];

    RectTransform rectTransform;
    MovementController movement;

    float halfHeight;
    float scaledHalfHeight;

    Vector2 prevScreenSize;
    float prevHandlePos;

    ChartEditor editor;

    // Value between 0 and 1
    public float handlePosRound
    {
        get
        {
            return (handle.transform.localPosition.y.Round(2) + halfHeight.Round(2)) / rectTransform.rect.height.Round(2);
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
            return (handle.transform.localPosition.y + halfHeight) / rectTransform.rect.height;
        }
        set
        {
            handle.transform.localPosition = handlePosToLocal(value);
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        halfHeight = rectTransform.rect.height / 2.0f;
        scaledHalfHeight = halfHeight * transform.lossyScale.y;

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();

        prevScreenSize = new Vector2(Screen.width, Screen.height);
        prevHandlePos = handlePos;
    }

    void Start()
    {
        sectionIndicatorParent = new GameObject("Section Indicators");
        sectionIndicatorParent.transform.SetParent(this.transform.parent);
        sectionIndicatorParent.transform.localPosition = Vector3.zero;
        sectionIndicatorParent.transform.localScale = new Vector3(1, 1, 1);
        sectionIndicatorParent.transform.SetSiblingIndex(1);

        starpowerIndicatorParent = new GameObject("Starpower Indicators");
        starpowerIndicatorParent.transform.SetParent(this.transform.parent);
        starpowerIndicatorParent.transform.localPosition = Vector3.zero;
        starpowerIndicatorParent.transform.localScale = new Vector3(1, 1, 1);
        starpowerIndicatorParent.transform.SetSiblingIndex(1);

        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        // Create section pool
        for (int i = 0; i < sectionIndicatorPool.Length; ++i)
        {
            GameObject sectionIndicator = Instantiate(sectionIndicatorPrefab);
            sectionIndicator.transform.SetParent(sectionIndicatorParent.transform);
            sectionIndicator.transform.localScale = new Vector3(1, 1, 1);

            sectionIndicatorPool[i] = sectionIndicator.GetComponent<SectionGuiController>();
            sectionIndicatorPool[i].handle = this;     
            sectionIndicatorPool[i].gameObject.SetActive(false);
        }

        // Create starpower pool
        for (int i = 0; i < starpowerIndicatorPool.Length; ++i)
        {
            GameObject spIndicator = Instantiate(starpowerIndicatorPrefab);
            spIndicator.transform.SetParent(starpowerIndicatorParent.transform);
            spIndicator.transform.localScale = new Vector3(1, 1, 1);

            starpowerIndicatorPool[i] = spIndicator.GetComponent<StarpowerGUIController>();
            starpowerIndicatorPool[i].handle = this;
            starpowerIndicatorPool[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        halfHeight = rectTransform.rect.height / 2.0f;
        scaledHalfHeight = halfHeight * transform.lossyScale.y;

        percentage.text = ((int)(handlePosRound * 100)).ToString() + "%";

        // Set the sections
        int i;
        for (i = 0; i < editor.currentSong.sections.Length; ++i)
        {
            if (i < sectionIndicatorPool.Length && editor.currentSong.sections[i].time <= editor.currentSong.length)
            {
                sectionIndicatorPool[i].section = editor.currentSong.sections[i];
                sectionIndicatorPool[i].gameObject.SetActive(true);
            }
            else
            { 
                break;
            }
        }

        while (i < sectionIndicatorPool.Length)
        {
            sectionIndicatorPool[i++].gameObject.SetActive(false);
        }

        // Set the sp
        for (i = 0; i < editor.currentChart.starPower.Length; ++i)
        {
            if (i < starpowerIndicatorPool.Length && editor.currentChart.starPower[i].time <= editor.currentSong.length)
            {
                starpowerIndicatorPool[i].starpower = editor.currentChart.starPower[i];
                starpowerIndicatorPool[i].gameObject.SetActive(true);
            }
            else
            {               
                break;
            }
        }

        while (i < starpowerIndicatorPool.Length)
        {
            starpowerIndicatorPool[i++].gameObject.SetActive(false);
        }

        if (prevScreenSize.x != Screen.width || prevScreenSize.y != Screen.height)
        {
            handlePos = prevHandlePos;
        }

        prevScreenSize = new Vector2(Screen.width, Screen.height);
        prevHandlePos = handlePos;
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
            {
                pos = handle.transform.localPosition;
                pos.y = handlePosToLocal(1).y;
                handle.transform.localPosition = pos;
            }
            else if (pos.y < transform.position.y - scaledHalfHeight)
            {
                pos = handle.transform.localPosition;
                pos.y = handlePosToLocal(0).y;
                handle.transform.localPosition = pos;
            }
            else
                handle.transform.position = pos;
        }
        MovementController.explicitChartPos = null;
    }

    public Vector3 handlePosToLocal(float pos)
    {
        // Audio may be shorter than the chart
        /*if (pos > 1)
            pos = 1;
        else */if (pos < 0)
            pos = 0;
        return new Vector3(handle.transform.localPosition.x, pos * rectTransform.rect.height - halfHeight, handle.transform.localPosition.z);
    }

    float minTimeRange = 0;
    float maxTimeRange = 300; // editor.currentSong.length

    public Vector3? TimeToLocalPosition(float timeInSeconds)
    {
        if (timeInSeconds < minTimeRange || timeInSeconds > maxTimeRange)
            return null;
        else
            return handlePosToLocal((timeInSeconds - minTimeRange) / (maxTimeRange - minTimeRange));
    }
}
