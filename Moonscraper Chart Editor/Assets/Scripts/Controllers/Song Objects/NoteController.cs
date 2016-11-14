using UnityEngine;
using System.Collections;

public class NoteController : MonoBehaviour {
    public Note note;
    public GameObject sustain;   

    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

    SpriteRenderer noteRenderer;
    Renderer sustainRen;

    void Awake()
    {      
        noteRenderer = GetComponent<SpriteRenderer>();
        sustainRen = sustain.GetComponent<Renderer>();
        sustainRen.material = new Material(sustainRen.sharedMaterial);
    }

    void OnMouseDown()
    {
        Debug.Log(note.position);
        Debug.Log(note.forced);     
    }

    void OnMouseOver()
    {
        // Delete the note
        if (Input.GetMouseButtonDown(1))
        {
            //RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(Camera.main.ScreenPointToRay(Input.mousePosition));
            Delete();
        }
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

        noteRenderer.sortingOrder = -(int)note.position;

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

        UpdateSustain();
    }

    public void UpdateSustain()
    {
        Note nextSameFret = FindNextSameFretWithinSustain();
        if (nextSameFret != null)
        {
            // Cap sustain
            note.sustain_length = nextSameFret.position - note.position;
        }

        float length = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length) - note.song.ChartPositionToWorldYPosition(note.position);

        Vector3 scale = sustain.transform.localScale;
        scale.y = length;
        sustain.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        sustain.transform.position = position;

        sustainRen.sharedMaterial = Globals.sustainColours[(int)note.fret_type];
    }

    public void Delete()
    {
        note.chart.Remove(note);

        // Update the previous note in the case of chords with 2 notes
        if (note.previous != null)
            note.previous.controller.UpdateNote();
        if (note.next != null)
            note.next.controller.UpdateNote();

        Debug.Log("Delete");
        Destroy(gameObject);
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

    Note FindNextSameFretWithinSustain()
    {
        int pos = SongObject.FindObjectPosition(note, note.chart.notes);
        if (pos != Globals.NOTFOUND)
        {
            ++pos;
            while (pos < note.chart.notes.Length)
            {
                Note next = note.chart.notes[pos];

                if (next.fret_type == note.fret_type && note.position + note.sustain_length > next.position)
                    return note.chart.notes[pos];
                else if (next.position >= note.position + note.sustain_length)
                    return null;

                ++pos;
            }
        }
        return null;
    }
}
