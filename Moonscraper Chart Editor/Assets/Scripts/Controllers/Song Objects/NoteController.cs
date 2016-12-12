//#define NOTE_TYPE_2D

using UnityEngine;
using System.Collections;

public class NoteController : SongObjectController {
    const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public Note note;
    public SustainController sustain;   

    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

#if NOTE_TYPE_2D
    protected SpriteRenderer noteRenderer;
#else
    protected Renderer noteRenderer;
    MeshFilter meshFilter;
#endif
    protected Renderer sustainRen;

    new void Awake()
    {
        base.Awake();
#if NOTE_TYPE_2D
        noteRenderer = GetComponent<SpriteRenderer>();
#else
        noteRenderer = GetComponent<Renderer>();
        meshFilter = GetComponent<MeshFilter>();
#endif
        sustainRen = sustain.GetComponent<Renderer>();
    }

    public override void OnSelectableMouseOver()
    {
        // Delete the object on erase tool
        if (Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButton(0) && Globals.applicationMode == Globals.ApplicationMode.Editor)
        {
            if (Input.GetButton("ChordSelect"))
            {
                Note[] chordNotes = note.GetChord();
                foreach (Note chordNote in chordNotes)
                {
                    if (chordNote.controller != null)
                    {
                        chordNote.controller.Delete();
                    }
                }
            }
            else
                Delete();
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0))
        {
            if (Input.GetButton("ChordSelect"))
            {
                Note[] chordNotes = note.GetChord();
                foreach (Note chordNote in chordNotes)
                {
                    if (chordNote.controller != null)
                    {
                        createPlaceNote(chordNote.controller);
                    }
                }
            }
            else
            {
                createPlaceNote(this);
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

    void createPlaceNote(NoteController nCon)
    {
        editor.currentSelectedObject = nCon.note;

        // Pass note data to a ghost note
        GameObject moveNote = Instantiate(editor.ghostNote);

        moveNote.name = "Moving note";
        Destroy(moveNote.GetComponent<PlaceNote>());
        MoveNote moveNoteController = moveNote.AddComponent<MoveNote>();

        
        moveNoteController.Init(nCon.note);
        moveNote.SetActive(true);
        moveNoteController.horizontalMouseOffset = nCon.gameObject.transform.position.x - snapToNearestHorizontalNotePos(((Vector2)Mouse.world2DPosition).x);

        // Delete note
        nCon.Delete();
    }

    public void SustainDrag()
    {
        uint snappedChartPos;
        ChartEditor.editOccurred = true;

        if (Mouse.world2DPosition != null && ((Vector2)Mouse.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(note.song.WorldYPositionToChartPosition(((Vector2)Mouse.world2DPosition).y), Globals.step, note.song.resolution);
        }
        else
        {
            snappedChartPos = Snapable.ChartPositionToSnappedChartPosition(note.song.WorldYPositionToChartPosition(editor.mouseYMaxLimit.position.y), Globals.step, note.song.resolution);
        }

        if (snappedChartPos > note.position)
            note.sustain_length = snappedChartPos - note.position;
        else
            note.sustain_length = 0;
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
#if NOTE_TYPE_2D
            BoxCollider2D hitBox = GetComponent<BoxCollider2D>();
            if (hitBox)
                hitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y);
#else
            BoxCollider hitBox = GetComponent<BoxCollider>();
            if (hitBox)
                hitBox.size = new Vector3(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y, hitBox.size.z);
#endif
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
            if (note.fret_type == Note.Fret_Type.GREEN)
                Debug.Log("Here");
            uint endPosition = note.position + note.sustain_length;

            if ((note.position >= editor.minPos && note.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (note.position < editor.minPos && endPosition >= editor.maxPos))
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

            // Update note visuals
            noteRenderer.sortingOrder = -(int)note.position;
#if NOTE_TYPE_2D
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
#else
            // Update mesh
            
            if (note.fret_type == Note.Fret_Type.OPEN)
                meshFilter.sharedMesh = Globals.openModel.sharedMesh;
            else
                meshFilter.sharedMesh = Globals.standardModel.sharedMesh;

            Material[] materials;
            switch (noteType)
            {
                case (Note.Note_Type.HOPO):
                    materials = Globals.hopoRenderer.sharedMaterials;

                    if (specialType == Note.Special_Type.STAR_POW)
                        materials[1] = Globals.spTemp;
                    else
                        materials[1] = Globals.strumColors[(int)note.fret_type];
                    break;
                case (Note.Note_Type.TAP):
                    materials = Globals.tapRenderer.sharedMaterials;

                    if (specialType == Note.Special_Type.STAR_POW)
                        materials[1] = Globals.spTapTemp;
                    else
                        materials[1] = Globals.tapColors[(int)note.fret_type];
                    break;
                default:    // strum
                    materials = Globals.strumRenderer.sharedMaterials;

                    if (specialType == Note.Special_Type.STAR_POW)
                        materials[1] = Globals.spTemp;
                    else
                        materials[1] = Globals.strumColors[(int)note.fret_type];
                    break;
            }
            noteRenderer.sharedMaterials = materials;
#endif

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

    float snapToNearestHorizontalNotePos(float pos)
    {
        // CHART_CENTER_POS + (int)note.fret_type - 2
        if (pos < CHART_CENTER_POS - 0.5f)
        {
            // -2
            if (pos < CHART_CENTER_POS - 1.5f)
                return CHART_CENTER_POS - 2;
            // -1
            else
            {
                return CHART_CENTER_POS - 1;
            }
        }
        else
        {
            // 0, 1 or 2
            if (pos > CHART_CENTER_POS + 1.5f)
                return CHART_CENTER_POS + 2;
            else if (pos > CHART_CENTER_POS + 0.5f)
                return CHART_CENTER_POS + 1;
            else
                return CHART_CENTER_POS;
        }
    }
}
