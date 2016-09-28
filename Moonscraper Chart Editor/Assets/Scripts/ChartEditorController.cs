using UnityEngine;
using UnityEngine.UI;
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

    // Use this for initialization
    void Start () {
        currentSong = new Song();
        initPos = transform.position;
        scrollBar.value = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (scrollDelta == 0)
        {
            scrollDelta = Input.mouseScrollDelta.y;
        }

        if (scrollDelta != 0)
        {
            // Mouse scroll movement
            transform.position = new Vector3(transform.position.x, transform.position.y + scrollDelta, transform.position.z);

            if (transform.position.y < initPos.y)
                transform.position = initPos;

            // Update the content height
            UpdateContentHeight(transform.position.y + Camera.main.orthographicSize - initPos.y);

            // Update the scroll value
            scrollBar.value = (transform.position.y - initPos.y) / (content.sizeDelta.y * content.transform.lossyScale.y);
        }

        // Update for grabbing the scroll bar
        else
        {
            // Scales the ditance moved to the size of the content height
            float distanceScale = 1 - 2.0f * Camera.main.orthographicSize / (content.rect.height * content.lossyScale.y);

            // Grabbing the scrollbar
            float pos = content.rect.height * content.lossyScale.y * scrollBar.value;// * distanceScale;
            
            // Apply the position
            transform.position = new Vector3(transform.position.x, pos + initPos.y, transform.position.z);

            // Update the content height
            UpdateContentHeight(transform.position.y + Camera.main.orthographicSize - initPos.y);
        } 
    }

    void UpdateContentHeight(float maxHeight)
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

    public void LoadChart()
    {
        try
        {
            currentSong = new Song(UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart"));

            // Remove notes from previous chart
            foreach (GameObject note in GameObject.FindGameObjectsWithTag("Note"))
            {
                Destroy(note);
            }

            // Add notes for current chart
            CreateChartObjects(currentSong.expert_single);

            songNameText.text = currentSong.name;

            transform.position = initPos;

            Debug.Log(Utility.BinarySearchChartExactNote(currentSong.expert_single[15], currentSong.expert_single.ToArray()));
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            // Most likely closed the window explorer, just ignore for now.
        }
    }

    void CreateChartObjects (Chart chart, GameObject notePrefab)
    {
        GameObject notes = new GameObject();
        notes.name = "Notes";

        foreach (Note note in chart.notes)
        {
            // Convert the chart data into gameobjects
            GameObject noteObject = Instantiate(notePrefab);
            noteObject.transform.position = new Vector3(Note.FretTypeToNoteNumber(note.fret_type) - 2, note.position * 0.01f, 0);
            noteObject.transform.parent = notes.transform;
            SpriteRenderer ren = noteObject.GetComponent<SpriteRenderer>();
            ren.sprite = normalSprites[Note.FretTypeToNoteNumber(note.fret_type)];
        }
    }

    void CreateChartObjects (Chart chart)
    {
        CreateChartObjects(chart, notePrefab);
    }
}
