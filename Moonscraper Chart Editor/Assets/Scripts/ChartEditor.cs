using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ChartEditor : MonoBehaviour {
    public GameObject notePrefab;
    public Text songNameText;
    public Transform strikeline;

    AudioSource musicSource;

    public Song currentSong { get; private set; }
    public Chart currentChart { get; private set; }
    string currentFileName = string.Empty;

    public MovementController movement;

    // Use this for initialization
    void Start () {
        currentSong = new Song();
        currentChart = currentSong.expert_single;
        musicSource = GetComponent<AudioSource>();
    }

    // Wrapper function
    public void LoadChart()
    {
        StartCoroutine(_LoadChart());
    }

    public void Save()
    {
        if (currentSong != null)
            currentSong.Save("test.chart");
    }

    public void Play()
    {
        float strikelinePos = strikeline.position.y;
        musicSource.time = Song.WorldYPositionToTime(strikelinePos) + currentSong.offset;       // No need to add audio calibration as position is base on the strikeline position

        movement.movementMode = MovementController.MovementMode.Playing;
        musicSource.Play();
    }

    public void Stop()
    {
        movement.movementMode = MovementController.MovementMode.Editor;
        musicSource.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown("u"))
        {
            movement.SetPosition(0);
            Debug.Log("Set");
        }
    }

    IEnumerator _LoadChart()
    {
        try
        {
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart");
            currentSong = new Song(currentFileName);

            // Remove notes from previous chart
            foreach (GameObject note in GameObject.FindGameObjectsWithTag("Note"))
            {
                Destroy(note);
            }

            currentChart = currentSong.expert_single;

            // Add notes for current chart
            CreateChartObjects(currentChart);

            songNameText.text = currentSong.name;

            movement.SetPosition(0);
        }
        catch (System.Exception e)
        {
            // Most likely closed the window explorer, just ignore for now.
            currentFileName = string.Empty;
            currentSong = new Song();
            Debug.LogError(e.Message);
        } 

        while (currentSong.musicStream != null && currentSong.musicStream.loadState == AudioDataLoadState.Loading)
        {
            Debug.Log("Loading audio...");
            yield return null;
        }
        
        if (currentSong.musicStream != null)
        {
            musicSource.clip = currentSong.musicStream;
        }
    }

    public void AddNewNoteToCurrentChart(Note note, GameObject parent)
    {
        // Insert note into current chart
        int position = currentChart.Add(note);

        // Create note object
        NoteController controller = CreateNoteObject(note, parent);

        // Update the linked list
        if (position > 0)
        {
            controller.prevNote = currentChart.FindPreviousNote(position);
            controller.prevNote.controller.nextNote = controller.note;
        }
        if (position < currentChart.Length - 1)
        {
            controller.nextNote = currentChart.FindNextNote(position);
            controller.nextNote.controller.prevNote = controller.note;
        }
    }

    public void DeleteNoteFromCurrentChart(NoteController controller)
    {
        // Update the linked list
        if (controller.prevNote != null)
            controller.prevNote.controller.nextNote = controller.nextNote;
        if (controller.nextNote != null)
            controller.nextNote.controller.prevNote = controller.prevNote;

        // Remove note from the chart data
        if (currentChart.Remove(controller.note))
            Debug.Log("Note successfully removed");
        else
            Debug.LogError("Note was not removed from data");

        // Remove the note from the scene
        Destroy(controller.gameObject);
    }

    GameObject CreateChartObjects(Chart chart, GameObject notePrefab)
    {
        GameObject parent = new GameObject();
        parent.name = "Notes";

        Note[] notes = chart.GetNotes();

        for (int i = 0; i < notes.Length; ++i)
        {
            NoteController controller = CreateNoteObject(notes[i], parent);

            // Join the linked list
            if (i > 0)
                controller.prevNote = notes[i - 1];
            if (i < notes.Length - 1)
                controller.nextNote = notes[i + 1];

            controller.UpdateNote();
        }

        return parent;
    }

    GameObject CreateChartObjects(Chart chart)
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

        // Link controller and note together
        controller.Init(note);

        return controller;
    }
}
