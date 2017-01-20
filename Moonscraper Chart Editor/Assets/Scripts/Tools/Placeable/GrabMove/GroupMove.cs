using UnityEngine;
using System.Collections;
using System.Linq;

public class GroupMove : Snapable {
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
    NoteController[] noteControllers = new NoteController[POOL_SIZE];
    StarpowerController[] starpowerControllers = new StarpowerController[POOL_SIZE];
    BPMController[] bpmControllers = new BPMController[POOL_SIZE];
    TimesignatureController[] tsControllers = new TimesignatureController[POOL_SIZE];
    SectionController[] sectionControllers = new SectionController[POOL_SIZE];

    void Start()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
        {
            noteControllers[i] = Instantiate(notePrefab).GetComponent<NoteController>();
            noteControllers[i].gameObject.SetActive(false);

            starpowerControllers[i] = Instantiate(spPrefab).GetComponent<StarpowerController>();
            starpowerControllers[i].gameObject.SetActive(false);

            bpmControllers[i] = Instantiate(bpmPrefab).GetComponent<BPMController>();
            bpmControllers[i].gameObject.SetActive(false);

            tsControllers[i] = Instantiate(tsPrefab).GetComponent<TimesignatureController>();
            tsControllers[i].gameObject.SetActive(false);

            sectionControllers[i] = Instantiate(sectionPrefab).GetComponent<SectionController>();
            sectionControllers[i].gameObject.SetActive(false);
        }
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

        // Assign in-view songObjects to gameobjects
        int notePos = 0, spPos = 0, bpmPos = 0, tsPos = 0, sectionPos = 0;
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
}
