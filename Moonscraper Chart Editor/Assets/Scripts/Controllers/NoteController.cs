using UnityEngine;
using System.Collections;

public class NoteController : MonoBehaviour {
    public Note note;

    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    public Note.Special_Type specialType = Note.Special_Type.NONE;

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

        // Note Type
        if ((note.flags & Note.Flags.TAP) == Note.Flags.TAP)
        {
            noteType = Note.Note_Type.TAP;
        }
        else
        {
            if (IsHopo)
                noteType = Note.Note_Type.HOPO;
            else
                noteType = Note.Note_Type.STRUM;
        }

        // Sprite
        switch (noteType)
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
            if (note.previous != null && note.previous.position == note.position)
                return true;
            else if (note.next != null && note.next.position == note.position)
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

            if (!IsChord && note.previous != null)
            {
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (note.previous.controller.IsChord || (!note.previous.controller.IsChord && note.fret_type != note.previous.fret_type))
                {
                    // Check distance from previous note 
                    const int HOPODistance = 95;

                    if (note.position - note.previous.position < HOPODistance)
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
