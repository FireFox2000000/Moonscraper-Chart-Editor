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

    MovementController movement;

    // Use this for initialization
    void Awake () {
        currentSong = new Song();
        currentChart = currentSong.expert_single;
        musicSource = GetComponent<AudioSource>();

        movement = GameObject.FindGameObjectWithTag("Movement").GetComponent<MovementController>();
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

        movement.applicationMode = MovementController.ApplicationMode.Playing;
        musicSource.Play();
    }

    public void Stop()
    {
        movement.applicationMode = MovementController.ApplicationMode.Editor;
        musicSource.Stop();
    }

    IEnumerator _LoadChart()
    {
        float totalLoadTime = 0;

        try
        {
            currentFileName = UnityEditor.EditorUtility.OpenFilePanel("Load Chart", "", "chart");

            totalLoadTime = Time.realtimeSinceStartup;

            currentSong = new Song(currentFileName);
            Debug.Log("Song load time: " + (Time.realtimeSinceStartup - totalLoadTime));

            float objectLoadTime = Time.realtimeSinceStartup;

            // Remove notes from previous chart
            foreach (GameObject note in GameObject.FindGameObjectsWithTag("Note"))
            {
                Destroy(note);
            }

            currentChart = currentSong.expert_single;

            // Add notes for current chart
            CreateChartObjects(currentChart);

            Debug.Log("Chart objects load time: " + (Time.realtimeSinceStartup - objectLoadTime));

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

        Debug.Log("Total load time: " + (Time.realtimeSinceStartup - totalLoadTime));
    }

    public void AddNewNoteToCurrentChart(Note note, GameObject parent)
    {
        // Insert note into current chart
        int position = currentChart.Add(note);

        // Create note object
        NoteController controller = CreateNoteObject(note, parent);
    }

    public void DeleteNoteFromCurrentChart(NoteController controller)
    {
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

        Note[] notes = chart.notes;

        for (int i = 0; i < notes.Length; ++i)
        {
            NoteController controller = CreateNoteObject(notes[i], parent);

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
