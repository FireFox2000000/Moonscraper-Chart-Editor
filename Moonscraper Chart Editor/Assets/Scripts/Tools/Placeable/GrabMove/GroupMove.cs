using UnityEngine;
using System.Collections;
using System.Linq;

public class GroupMove : ToolObject
{
    public GameObject notePrefab;
    public GameObject spPrefab;
    public GameObject bpmPrefab;
    public GameObject tsPrefab;
    public GameObject sectionPrefab;

    SongObject[] originalSongObjects = new ChartObject[0];
    SongObject[] movingSongObjects = new ChartObject[0];
    
    Vector2 initMousePos = Vector2.zero;
    uint initObjectSnappedChartPos = 0;

    const int POOL_SIZE = 50;
    
    NoteController[] noteControllers;
    StarpowerController[] starpowerControllers;
    BPMController[] bpmControllers;
    TimesignatureController[] tsControllers;
    SectionController[] sectionControllers;

    void Start()
    {
        GameObject groupMovePool = new GameObject("Group Move Object Pool");

        GameObject notes;
        noteControllers = (NoteController[])ChartEditor.SOConInstanciate(editor.notePrefab, POOL_SIZE, out notes);
        notes.name = "Group Move notes";
        notes.transform.SetParent(groupMovePool.transform);

        GameObject starpowers;
        starpowerControllers = (StarpowerController[])ChartEditor.SOConInstanciate(editor.starpowerPrefab, POOL_SIZE, out starpowers);
        starpowers.name = "Group Move SP";
        starpowers.transform.SetParent(groupMovePool.transform);

        GameObject bpms;
        bpmControllers = (BPMController[])ChartEditor.SOConInstanciate(editor.bpmPrefab, POOL_SIZE, out bpms);
        bpms.name = "Group Move BPMs";
        bpms.transform.SetParent(groupMovePool.transform);

        GameObject timesignatures;
        tsControllers = (TimesignatureController[])ChartEditor.SOConInstanciate(editor.bpmPrefab, POOL_SIZE, out timesignatures);
        timesignatures.name = "Group Move TSs";
        timesignatures.transform.SetParent(groupMovePool.transform);

        GameObject sections;
        sectionControllers = (SectionController[])ChartEditor.SOConInstanciate(editor.sectionPrefab, POOL_SIZE, out sections);
        sections.name = "Group Move sections";
        sections.transform.SetParent(groupMovePool.transform);
    }

    // Update is called once per frame
    protected override void Update () {
        UpdateSnappedPos();

        if (Mouse.world2DPosition != null)
        {
            Vector2 mousePosition = (Vector2)Mouse.world2DPosition;
            int chartPosOffset = (int)(objectSnappedChartPos - initObjectSnappedChartPos);
            bool hitStartOfChart = false;

            // Guard for chart limit, if the offset was negative, yet the position becomes greater
            if (movingSongObjects.Length > 0 && chartPosOffset < 0 && (uint)((int)originalSongObjects[0].position + chartPosOffset) > originalSongObjects[0].position)
            {
                hitStartOfChart = true;
            }

            // Update the new positions of all the notes that have been moved
            for (int i = 0; i < movingSongObjects.Length; ++i)
            {
                // Alter X position
                if ((SongObject.ID)movingSongObjects[i].classID == SongObject.ID.Note)
                {
                    Note note = movingSongObjects[i] as Note;
                    if (note.fret_type != Note.Fret_Type.OPEN)
                    {
                        mousePosition.x -= initMousePos.x;      // Offset
                        note.fret_type = PlaceNote.XPosToFretType(mousePosition.x);
                    }
                }

                // Alter chart position
                if (!hitStartOfChart)
                    movingSongObjects[i].position = (uint)((int)originalSongObjects[i].position + chartPosOffset);
                else
                {
                    movingSongObjects[i].position = originalSongObjects[i].position - originalSongObjects[0].position;
                }
            }
        }
	}

    void AddSongObjects()
    {
        throw new System.NotImplementedException();     
        // Need to remember to undo/redo. This current will only work once object pools are implemented.
        // Check to see what the current offset is to decide how to record
        // Will also need to check for overwrites

        foreach (SongObject songObject in movingSongObjects)
        {
            if (songObject.GetType().IsSubclassOf(typeof(ChartObject)))
            {
                editor.currentChart.Add((ChartObject)songObject, false);
            }
            else
            {
                editor.currentSong.Add(songObject, false);
            }
        }

        editor.currentSong.updateArrays();
        editor.currentChart.updateArrays();

        originalSongObjects = new ChartObject[0];
        movingSongObjects = new ChartObject[0];
    }

    public void SetSongObjects(SongObject[] songObjects)
    {
        originalSongObjects = new SongObject[songObjects.Length];
        movingSongObjects = new SongObject[songObjects.Length];

        initObjectSnappedChartPos = objectSnappedChartPos;

        int lastNotePos = -1;
        for (int i = 0; i < songObjects.Length; ++i)
        {
            originalSongObjects[i] = songObjects[i].Clone();
            movingSongObjects[i] = songObjects[i].Clone();

            // Rebuild linked list
            if ((SongObject.ID)songObjects[i].classID == SongObject.ID.Note)
            {
                if (lastNotePos >= 0)
                {
                    ((Note)originalSongObjects[i]).previous = ((Note)originalSongObjects[lastNotePos]);
                    ((Note)originalSongObjects[lastNotePos]).next = ((Note)originalSongObjects[i]);

                    ((Note)movingSongObjects[i]).previous = ((Note)movingSongObjects[lastNotePos]);
                    ((Note)movingSongObjects[lastNotePos]).next = ((Note)movingSongObjects[i]);
                }

                lastNotePos = i;
            }
        }
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        originalSongObjects = new ChartObject[0];
        movingSongObjects = new ChartObject[0];
    }
}
