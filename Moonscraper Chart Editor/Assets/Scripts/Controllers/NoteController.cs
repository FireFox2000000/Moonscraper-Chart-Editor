using UnityEngine;
using System.Collections;

public class NoteController : MonoBehaviour {
    public Note note;
    public Note prevNote = null;        // Linked list style
    public Note nextNote = null;

    SpriteRenderer noteRenderer;
    
    void Awake()
    {
        noteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        Debug.Log(note.position);
        Debug.Log(note.forced);
    }

    public void Init(Note note)
    {
        this.note = note;
        this.note.controller = this;
    }

    public void UpdateNote()
    {
        // Position
        transform.position = new Vector3((int)note.fret_type - 2, note.song.ChartPositionToWorldYPosition(note.position), 0);

        // Type
        if ((note.flags & Note.Flags.TAP) == Note.Flags.TAP)
        {
            note.note_type = Note.Note_Type.TAP;
        }
        else
        {
            if (IsHopo)
                note.note_type = Note.Note_Type.HOPO;
            else
                note.note_type = Note.Note_Type.STRUM;
        }

        // Sprite
        switch (note.note_type)
        {
            case (Note.Note_Type.HOPO):
                noteRenderer.sprite = Globals.hopoSprites[(int)note.fret_type];
                break;
            case (Note.Note_Type.TAP):
                noteRenderer.sprite = Globals.tapSprites[(int)note.fret_type];
                break;
            default:
                noteRenderer.sprite = Globals.normalSprites[(int)note.fret_type];
                break;
        }
    }

    public bool IsChord
    {
        get
        {
            if (prevNote != null && prevNote.position == note.position)
                return true;
            else if (nextNote != null && nextNote.position == note.position)
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
                if (prevNote.controller.IsChord || (!prevNote.controller.IsChord && note.fret_type != prevNote.fret_type))
                {
                    // Check distance from previous note 
                    const int HOPODistance = 95;

                    if (note.position - prevNote.position < HOPODistance)
                        HOPO = true;
                }
            }

            // Check if forced
            if (note.forced)
                HOPO = !HOPO;

            return HOPO;
        }
    }
}
