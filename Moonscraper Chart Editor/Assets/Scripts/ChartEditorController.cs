using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ChartEditorController : MonoBehaviour {
    public GameObject notePrefab;
    public Sprite[] normalSprites = new Sprite[5];
    public UnityEngine.UI.Text songNameText;

    Vector3 initPos;

	// Use this for initialization
	void Start () {
        initPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(transform.position.x, transform.position.y + Input.mouseScrollDelta.y, transform.position.z);

        if (transform.position.y < initPos.y)
            transform.position = initPos;
    }

    public void LoadChart()
    {
        try
        {
            Song currentSong = new Song(UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart"));

            // Remove notes from previous chart
            foreach (GameObject note in GameObject.FindGameObjectsWithTag("Note"))
            {
                Destroy(note);
            }

            // Add notes for current chart
            CreateChartObjects(currentSong.expert_single);

            songNameText.text = currentSong.name;

            transform.position = initPos;

            Debug.Log(Utility.BinarySearchPos(currentSong.expert_single[15], currentSong.expert_single.ToArray()));
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            // Most likely closed the window explorer, just ignore for now.
        }
    }

    void CreateChartObjects (Note[] chart, GameObject notePrefab)
    {
        GameObject notes = new GameObject();
        notes.name = "Notes";

        foreach (Note note in chart)
        {
            // Convert the chart data into gameobjects
            GameObject noteObject = Instantiate(notePrefab);
            noteObject.transform.position = new Vector3(Note.FretTypeToNoteNumber(note.fret_type) - 2, note.position * 0.01f, 0);
            noteObject.transform.parent = notes.transform;
            SpriteRenderer ren = noteObject.GetComponent<SpriteRenderer>();
            ren.sprite = normalSprites[Note.FretTypeToNoteNumber(note.fret_type)];
        }
    }

    void CreateChartObjects (List<Note> chart)
    {
        CreateChartObjects(chart.ToArray(), notePrefab);
    }
}
