using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ChartEditorController : MonoBehaviour {
    public GameObject notePrefab;
    public Scrollbar scrollBar;
    public RectTransform content;
    public Sprite[] normalSprites = new Sprite[5];
    public Text songNameText;

    Vector3 initPos;

    float scrollDelta = 0;

    Song currentSong;
    Chart currentChart;

    // Program options
    float mouseScrollSensitivity = 0.5f;

    // Use this for initialization
    void Start () {
        currentSong = new Song();
        currentChart = currentSong.expert_single;
        initPos = transform.position;
        scrollBar.value = 0;
        UpdatePosBasedScrollValue();
    }

    // Update is called once per frame
	void Update () {
        if (scrollDelta == 0)
        {
            scrollDelta = Input.mouseScrollDelta.y;
        }

        // Position changes scroll bar value
        if (scrollDelta != 0)
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

    void UpdateScrollValueBasedPos()
    {
        UpdateContentHeight();

        // Update the scroll value
        scrollBar.value = (transform.position.y - initPos.y) / (content.sizeDelta.y * content.transform.lossyScale.y);
    }

    void UpdatePosBasedScrollValue()
    {
        // Scales the ditance moved to the size of the content height
        float distanceScale = 1 - 2.0f * Camera.main.orthographicSize / (content.rect.height * content.lossyScale.y);
        
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
        if (currentChart != null)
        {
            if (currentChart.Length > 0 && currentChart[currentChart.Length - 1].position * Globals.zoom > user_pos)
                max = currentChart[currentChart.Length - 1].position * Globals.zoom;
            ContentHeight(content, max);
        }
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
        if (Event.current.type == EventType.ScrollWheel)
        {
            scrollDelta = -Event.current.delta.y;
        }
        else
        {
            scrollDelta = 0;
        }
    }

    // Wrapper function
    public void LoadChart()
    {
        StartCoroutine(_LoadChart());
    }

    public IEnumerator _LoadChart()
    {
        try
        {
            currentSong = new Song(UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart"));

            // Remove notes from previous chart
            foreach (GameObject note in GameObject.FindGameObjectsWithTag("Note"))
            {
                Destroy(note);
            }

            currentChart = currentSong.expert_single;

            // Add notes for current chart
            CreateChartObjects(currentChart); 

            songNameText.text = currentSong.name;

            transform.position = initPos;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            // Most likely closed the window explorer, just ignore for now.
        }
        yield return null;
        scrollBar.value = 0; 
    }

    public void AddNewNoteToCurrentChart(Note note, GameObject parent)
    {
        // Insert note into current chart
        int position = currentChart.Add(note);

        // Create note object
        NoteController controller = CreateNoteObject(note, parent);

        if (position > 0)
            controller.prevNote = currentChart.notes[position - 1];
        if (position < currentChart.notes.Count - 1)
            controller.nextNote = currentChart.notes[position + 1];
    }

    public void DeleteNoteFromCurrentChart(GameObject note)
    {
        NoteController controller = note.GetComponent<NoteController>();

        // Remove note from the chart data
        currentChart.Remove(controller.noteProperties);

        // Remove the note from the scene
        Destroy(note);
    }

    GameObject CreateChartObjects (Chart chart, GameObject notePrefab)
    {
        GameObject parent = new GameObject();
        parent.name = "Notes";

        for(int i = 0; i < chart.notes.Count; ++i)
        {
            NoteController controller = CreateNoteObject(chart.notes[i], parent);
            /*
            if (i > 0)
                controller.prevNote = chart.notes[i - 1];
            if (i < chart.notes.Count - 1)
                controller.nextNote = chart.notes[i + 1];
                */
        }

        return parent;
    }

    GameObject CreateChartObjects (Chart chart)
    {
        return CreateChartObjects(chart, notePrefab);
    }

    NoteController CreateNoteObject(Note note, GameObject parent = null)
    {
        
        // Convert the chart data into gameobject
        GameObject noteObject = Instantiate(notePrefab);   

        if (parent)
            noteObject.transform.parent = parent.transform;
        
        // Attach the note to the object
        NoteController controller = noteObject.GetComponent<NoteController>();
        controller.noteProperties = note;
        controller.UpdateNote();

        // Update sprite, eventually move this to the note controller
        SpriteRenderer ren = noteObject.GetComponent<SpriteRenderer>();
        ren.sprite = normalSprites[Note.FretTypeToNoteNumber(note.fret_type)];

        return controller;
    }
}
