using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroupMove : ToolObject
{
    SongObject[] originalSongObjects = new ChartObject[0];
    SongObject[] movingSongObjects = new ChartObject[0];
    
    Vector2 initMousePos = Vector2.zero;
    uint initObjectSnappedChartPos = 0;

    // Update is called once per frame
    
    protected override void Update () {
        if (movingSongObjects.Length > 0 && Input.GetMouseButtonUp(0))
        {
            AddSongObjects();
        }
        else
        {
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
                            float position = NoteController.GetXPos(0, originalSongObjects[i] as Note) + (mousePosition.x - initMousePos.x);      // Offset
                            note.fret_type = PlaceNote.XPosToFretType(position);
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

            // Enable objects into the pool
            
            editor.songObjectPoolManager.EnableNotes(movingSongObjects.OfType<Note>().ToArray());
            editor.songObjectPoolManager.EnableSP(movingSongObjects.OfType<Starpower>().ToArray());
            editor.songObjectPoolManager.EnableBPM(movingSongObjects.OfType<BPM>().ToArray());
            editor.songObjectPoolManager.EnableTS(movingSongObjects.OfType<TimeSignature>().ToArray());
            editor.songObjectPoolManager.EnableSections(movingSongObjects.OfType<Section>().ToArray());
        }
	}  

    public void AddSongObjects()
    {
        List<ActionHistory.Action> record = new List<ActionHistory.Action>();
        List<ActionHistory.Action> deleteRecord = new List<ActionHistory.Action>();

        // Need to remember to undo/redo. This current will only work once object pools are implemented.
        // Check to see what the current offset is to decide how to record
        // Will also need to check for overwrites
        // All relative to the original notes

        bool moved = false;

        for (int i = 0; i < movingSongObjects.Length; ++i)
        {
            ActionHistory.Action overwriteRecord;

            if (movingSongObjects[i] != originalSongObjects[i])
            {
                moved = true;
                deleteRecord.Add(new ActionHistory.Delete(originalSongObjects[i]));
            }
            
            switch ((SongObject.ID)movingSongObjects[i].classID)
            {
                case (SongObject.ID.Note):
                    record.AddRange(PlaceNote.AddObjectToCurrentChart((Note)movingSongObjects[i], editor, false, false));     // Capping
                    break;
                case (SongObject.ID.Starpower):
                    record.AddRange(PlaceStarpower.AddObjectToCurrentChart((Starpower)movingSongObjects[i], editor, false, false));       // Capping
                    break;
                case (SongObject.ID.BPM):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.bpms);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((BPM)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.TimeSignature):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.timeSignatures);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((TimeSignature)movingSongObjects[i], false);
                    break;
                case (SongObject.ID.Section):
                    overwriteRecord = PlaceSongObject.OverwriteActionHistory(movingSongObjects[i], editor.currentSong.sections);
                    if (record != null)
                        record.Add(overwriteRecord);
                    editor.currentSong.Add((Section)movingSongObjects[i], false);
                    break;
                default:
                    break;
            }     
        }

        editor.currentSelectedObjects = movingSongObjects;

        if (moved)
        {
            editor.actionHistory.Insert(deleteRecord.ToArray());
            editor.actionHistory.Insert(record.ToArray());
        }

        editor.currentSong.updateArrays();
        editor.currentChart.updateArrays();

        Reset();
    }

    void Reset()
    {
        originalSongObjects = new ChartObject[0];

        foreach (SongObject songObject in movingSongObjects)
        {
            if (songObject.controller)
                songObject.controller.gameObject.SetActive(false);
        }
        movingSongObjects = new ChartObject[0];
    }

    public void SetSongObjects(SongObject songObject)
    {
        SetSongObjects(new SongObject[] { songObject });
    }

    public void SetSongObjects(SongObject[] songObjects, bool delete = false)
    {
        float time = Time.realtimeSinceStartup;
        if (Mouse.world2DPosition != null)
            initMousePos = (Vector2)Mouse.world2DPosition;
        else
            initMousePos = Vector2.zero;

        editor.currentSelectedObject = null;
        Reset();

        originalSongObjects = songObjects;
        movingSongObjects = new SongObject[songObjects.Length];

        initObjectSnappedChartPos = objectSnappedChartPos;

        int lastNotePos = -1;
        for (int i = 0; i < songObjects.Length; ++i)
        {
            //originalSongObjects[i] = songObjects[i];
            movingSongObjects[i] = songObjects[i].Clone();

            if (delete)
                songObjects[i].Delete(false);

            // Rebuild linked list
            
            if ((SongObject.ID)songObjects[i].classID == SongObject.ID.Note)
            {
                if (lastNotePos >= 0)
                {
                    //((Note)originalSongObjects[i]).previous = ((Note)originalSongObjects[lastNotePos]);
                    //((Note)originalSongObjects[lastNotePos]).next = ((Note)originalSongObjects[i]);

                    ((Note)movingSongObjects[i]).previous = ((Note)movingSongObjects[lastNotePos]);
                    ((Note)movingSongObjects[lastNotePos]).next = ((Note)movingSongObjects[i]);
                }

                lastNotePos = i;
            }

            originalSongObjects[i].song = editor.currentSong;
            movingSongObjects[i].song = editor.currentSong;

            if (originalSongObjects[i].GetType().IsSubclassOf(typeof(ChartObject)))
            {
                ((ChartObject)originalSongObjects[i]).chart = editor.currentChart;
                ((ChartObject)movingSongObjects[i]).chart = editor.currentChart;
            }
        }

        Mouse.cancel = true;
        editor.currentSong.updateArrays();
        editor.currentChart.updateArrays();

        Debug.Log(Time.realtimeSinceStartup - time);
    }

    public override void ToolDisable()
    {
        base.ToolDisable();
        Reset();
    }
}
