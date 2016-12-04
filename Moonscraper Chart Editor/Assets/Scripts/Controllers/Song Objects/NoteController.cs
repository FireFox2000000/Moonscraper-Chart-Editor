using UnityEngine;
using System.Collections;

public class NoteController : SongObjectController {
    const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public Note note;
    public GameObject sustain;   

    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

    protected SpriteRenderer noteRenderer;
    protected Renderer sustainRen;

    new void Awake()
    {
        base.Awake();
        noteRenderer = GetComponent<SpriteRenderer>();
        sustainRen = sustain.GetComponent<Renderer>();
        sustainRen.material = new Material(sustainRen.sharedMaterial);
    }

    void OnMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            // Prevent note from snapping if the user is just clicking and not dragging
            if (prevMousePos != (Vector2)Input.mousePosition)
            {
                // Pass note data to a ghost note
                GameObject moveNote = Instantiate(editor.ghostNote);
                moveNote.SetActive(true);

                moveNote.name = "Moving note";
                Destroy(moveNote.GetComponent<PlaceNote>());
                moveNote.AddComponent<MoveNote>().Init(note);

                editor.currentSelectedObject = note;


                // Delete note
                Delete();
            }
            else
            {
                prevMousePos = Input.mousePosition;
            }
        }
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1))
        {
            if (Input.GetButton("ChordSelect"))
                ChordSustainDrag();
            else
                SustainDrag();
        }
    }

    public void SustainDrag()
    {
        if (Mouse.world2DPosition != null)
        {
            ChartEditor.editOccurred = true;

            uint snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(note.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, note.song.resolution);

            if (snappedChartPos > note.position)
                note.sustain_length = snappedChartPos - note.position;
            else
                note.sustain_length = 0;
        }
    }

    public void ChordSustainDrag()
    {
        if (Mouse.world2DPosition != null)
        {
            Note[] chordNotes = note.GetChord();
            foreach (Note chordNote in chordNotes)
            {
                if (chordNote.controller != null)
                {
                    chordNote.controller.SustainDrag();
                }
            }
        }
    }

    public void Init(Note note)
    {
        base.Init(note);
        this.note = note;
        this.note.controller = this;

        if (note.fret_type == Note.Fret_Type.OPEN)
        {
            // Apply scaling
            sustain.transform.localScale = new Vector3(OPEN_NOTE_SUSTAIN_WIDTH, sustain.transform.localScale.y, sustain.transform.localScale.z);
            BoxCollider2D hitBox = GetComponent<BoxCollider2D>();
            if (hitBox)
                hitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y);
        }

        if (note.IsChord)
        {
            Note[] chordNotes = SongObject.FindObjectsAtPosition(note.position, note.chart.notes);

            if (note.fret_type == Note.Fret_Type.OPEN)
            {

                // Check for non-open notes and delete
                foreach (Note chordNote in chordNotes)
                {
                    if (chordNote.fret_type != Note.Fret_Type.OPEN)
                    {
                        if (chordNote.controller != null)
                            chordNote.controller.Delete();
                        else
                            note.chart.Remove(chordNote);
                    }
                }
            }
        }
    }
    
    protected override void Update()
    {
#if false
        if (noteRenderer.isVisible || sustainRen.isVisible)
            UpdateSongObject();
        else if (note != null)
        {
            uint endPosition = note.position + note.sustain_length;

            if ((note.position > editor.minPos && note.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (note.position < editor.minPos && endPosition > editor.maxPos))
            {              
                UpdateSongObject();
            }
        }
#else
        if (note != null)
        {
            uint endPosition = note.position + note.sustain_length;

            if ((note.position >= editor.minPos && note.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (note.position < editor.minPos && endPosition > editor.maxPos))
            {
                UpdateSongObject();
            }
            else 
                gameObject.SetActive(false);
        }
        else 
            gameObject.SetActive(false);
#endif
    }

    public override void UpdateSongObject()
    {
        if (note.song != null)
        {
            // Position
            if (note.fret_type != Note.Fret_Type.OPEN)
                transform.position = new Vector3(CHART_CENTER_POS + (int)note.fret_type - 2, note.worldYPosition, 0);
            else
                transform.position = new Vector3(CHART_CENTER_POS, note.worldYPosition, 0);
            
            // Note Type
            if (note.fret_type != Note.Fret_Type.OPEN && (note.flags & Note.Flags.TAP) == Note.Flags.TAP)
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

            // Star power?
            specialType = Note.Special_Type.NONE;
            foreach (StarPower sp in note.chart.starPower)
            {
                if (sp.position == note.position || (sp.position <= note.position && sp.position + sp.length > note.position))
                {
                    specialType = Note.Special_Type.STAR_POW;
                }
                else if (sp.position > note.position)
                    break;
            }

            // Sprite
            noteRenderer.sortingOrder = -(int)note.position;
            switch (noteType)
            {
                case (Note.Note_Type.HOPO):
                    if (specialType == Note.Special_Type.STAR_POW)
                        noteRenderer.sprite = Globals.spHopoSprite[(int)note.fret_type];
                    else
                        noteRenderer.sprite = Globals.hopoSprites[(int)note.fret_type];
                    break;
                case (Note.Note_Type.TAP):
                    if (specialType == Note.Special_Type.STAR_POW)
                        noteRenderer.sprite = Globals.spTapSprite[(int)note.fret_type];
                    else
                        noteRenderer.sprite = Globals.tapSprites[(int)note.fret_type];
                    break;
                default:
                    if (specialType == Note.Special_Type.STAR_POW)
                        noteRenderer.sprite = Globals.spStrumSprite[(int)note.fret_type];
                    else
                        noteRenderer.sprite = Globals.strumSprites[(int)note.fret_type];
                    break;
            }

            UpdateSustain();
        }
    }

    public void UpdateSustain()
    {       
        Note nextFret;
        if (note.fret_type == Note.Fret_Type.OPEN)
            nextFret = note.next;
        else
            nextFret = FindNextSameFretWithinSustain();

        if (nextFret != null)
        {
            if (nextFret.position < note.position)
                note.sustain_length = 0;
            else if (note.position + note.sustain_length > nextFret.position)
                // Cap sustain
                note.sustain_length = nextFret.position - note.position;
        }
        
        UpdateSustainLength();       

        sustainRen.sharedMaterial = Globals.sustainColours[(int)note.fret_type];
    }

    public void UpdateSustainLength()
    {
        float length = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length) - note.song.ChartPositionToWorldYPosition(note.position);

        Vector3 scale = sustain.transform.localScale;
        scale.y = length;
        sustain.transform.localScale = scale;

        Vector3 position = transform.position;
        position.y += length / 2.0f;
        sustain.transform.position = position;
    }

    Note GetPreviousOfOpen(uint openNotePos, Note previousNote)
    {
        if (previousNote == null || previousNote.position != openNotePos || (!previousNote.IsChord && previousNote.position != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.position != openNotePos || (!nextNote.IsChord && nextNote.position != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
    }

    public override void Delete()
    {
        note.chart.Remove(note);

        // Update the previous note in the case of chords with 2 notes
        if (note.previous != null)
            note.previous.controller.UpdateSongObject();
        if (note.next != null)
            note.next.controller.UpdateSongObject();

        Destroy(gameObject);
    }

    public bool IsHopo
    {
        get
        {
            bool HOPO = false;

            if (!note.IsChord && note.previous != null)
            {
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (note.previous.IsChord || (!note.previous.IsChord && note.fret_type != note.previous.fret_type))
                {
                    // Check distance from previous note 
                    int HOPODistance = (int)(65 * note.song.resolution / Globals.STANDARD_BEAT_RESOLUTION);

                    if (note.position - note.previous.position <= HOPODistance)
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
        Note next = note.next;

        while (next != null)
        {
            if (next.fret_type == Note.Fret_Type.OPEN || (next.fret_type == note.fret_type && note.position + note.sustain_length > next.position))
                return next;
            else if (next.position >= note.position + note.sustain_length)
                return null;

            next = next.next;
        }

        return null;     
    }
}
