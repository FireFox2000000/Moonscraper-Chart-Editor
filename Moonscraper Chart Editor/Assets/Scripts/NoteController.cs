using UnityEngine;
using System.Collections;

public class NoteController : MonoBehaviour {

    public Note noteProperties;
    public Note prevNote = null;        // Linked list style
    public Note nextNote = null;

    SpriteRenderer noteRenderer;
    
    void Awake()
    {
        noteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        Debug.Log(noteProperties.position);
    }

    public void UpdateNote()
    {
        transform.position = new Vector3(Note.FretTypeToNoteNumber(noteProperties.fret_type) - 2, noteProperties.position * Globals.zoom, 0);
    }

    public bool ChordCheck()
    {
        if (prevNote != null && prevNote.position == noteProperties.position)
            return true;

        else if (nextNote != null && nextNote.position == noteProperties.position)
            return true;

        else
            return false;
    }

    public bool HOPOCheck()
    {
        bool HOPO = false;

        // Check if chord
        if (!ChordCheck())
        {
            // Check distance from previous note
            const int HOPODistance = 50;
            
            if (prevNote != null && noteProperties.position - prevNote.position > HOPODistance)
                HOPO = true;
        }

        // Check if forced
        if (noteProperties.forced)
            HOPO = !HOPO;
        return HOPO;
    }
}
