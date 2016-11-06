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
        // Position
        transform.position = new Vector3((int)noteProperties.fret_type - 2, noteProperties.position * Globals.zoom, 0);

        // Type
        if (noteProperties.note_type != Note.Note_Type.TAP)
        {
            if (IsHopo)
                noteProperties.note_type = Note.Note_Type.HOPO;
            else
                noteProperties.note_type = Note.Note_Type.NORMAL;
        }

        // Sprite
        switch (noteProperties.note_type)
        {
            case (Note.Note_Type.HOPO):
                noteRenderer.sprite = Globals.hopoSprites[(int)noteProperties.fret_type];
                break;
            case (Note.Note_Type.TAP):
                noteRenderer.sprite = Globals.tapSprites[(int)noteProperties.fret_type];
                break;
            default:
                noteRenderer.sprite = Globals.normalSprites[(int)noteProperties.fret_type];
                break;
        }
    }

    public bool IsChord
    {
        get
        {
            if (prevNote != null && prevNote.position == noteProperties.position)
                return true;
            else if (nextNote != null && nextNote.position == noteProperties.position)
                return true;
            else
                return false;
        }
    }

    public bool IsHopo
    {
        get
        {
            bool HOPO = false;

            if (!IsChord && prevNote != null)
            {
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (prevNote.controller.IsChord || (!prevNote.controller.IsChord && noteProperties.fret_type != prevNote.fret_type))
                {
                    // Check distance from previous note 
                    const int HOPODistance = 100;

                    if (noteProperties.position - prevNote.position < HOPODistance)
                        HOPO = true;
                }
            }

            // Check if forced
            if (noteProperties.forced)
                HOPO = !HOPO;

            return HOPO;
        }
    }
}
