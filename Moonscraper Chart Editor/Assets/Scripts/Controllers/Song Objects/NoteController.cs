//#define NOTE_TYPE_2D

using UnityEngine;
using System.Collections;

public class NoteController : SongObjectController {
    const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public Note note { get { return (Note)songObject; } set { songObject = value; } }
    public SustainController sustain;   

    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

#if NOTE_TYPE_2D
    protected SpriteRenderer noteRenderer;
#else
    protected Renderer noteRenderer;
    EdgeCollider2D noteHitCollider;
    MeshFilter meshFilter;
#endif
    protected SpriteRenderer sustainRen;

    public bool hit = false;
    public bool sustainBroken = false;
    public bool isActivated
    {
        get
        {
            if (noteRenderer && noteHitCollider && noteRenderer.enabled && noteHitCollider.enabled)
                return true;
            else
                return false;
        }
    }
    new void Awake()
    {
        base.Awake();
#if NOTE_TYPE_2D
        noteRenderer = GetComponent<SpriteRenderer>();
#else
        noteRenderer = GetComponent<Renderer>();
        noteHitCollider = GetComponentInChildren<EdgeCollider2D>();
        meshFilter = GetComponent<MeshFilter>();
#endif
        sustainRen = sustain.GetComponent<SpriteRenderer>();
    }

    public override void OnSelectableMouseDown()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            editor.currentSelectedObject = songObject;
        }

        // Delete the object on erase tool
        if ((Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor) ||
            (Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1)))
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
            {
                Delete();
            }
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            if (Input.GetButton("ChordSelect"))
            {
                Note[] chordNotes = note.GetChord();
                SongObject.sort(chordNotes);

                MoveNote previousInChord = null;
                foreach (Note chordNote in chordNotes)
                {
                    if (chordNote.controller != null)
                    {
                        MoveNote mCon = createPlaceNote(chordNote.controller);

                        if (previousInChord != null)
                        {
                            previousInChord.explicitNext = mCon.note;
                            mCon.explicitPrevious = previousInChord.note;
                        }

                        previousInChord = mCon;
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
            if (!Globals.extendedSustainsEnabled || Input.GetButton("ChordSelect"))
                sustain.ChordSustainDrag();
            else
                sustain.SustainDrag();
        }
    }

    MoveNote createPlaceNote(NoteController nCon)
    {
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

        return moveNoteController;
    }

    public void Init(Note note)
    {
        base.Init(note);
        this.note = note;
        this.note.controller = this;

        if (note.fret_type == Note.Fret_Type.OPEN)
        {
            // Apply scaling
            //sustain.transform.localScale = new Vector3(OPEN_NOTE_SUSTAIN_WIDTH, sustain.transform.localScale.y, sustain.transform.localScale.z);
            sustainRen.sprite = Globals.openSustainSprite;

            BoxCollider2D sustainHitBox = sustain.GetComponent<BoxCollider2D>();
            if (sustainHitBox)
                sustainHitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, sustainHitBox.size.y);

            // Adjust note hitbox size
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

        // Open notes overwrite here for initial loading
        if (note.fret_type == Note.Fret_Type.OPEN)
        {
            Note[] chordNotes = note.GetChord();

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

    public void standardOverwriteOpen()
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
        {
            Note[] chordNotes = SongObject.FindObjectsAtPosition(note.position, note.chart.notes);

            // Check for open notes and delete
            foreach (Note chordNote in chordNotes)
            {
                if (chordNote.fret_type == Note.Fret_Type.OPEN)
                {
                    if (chordNote.controller != null)
                        chordNote.controller.Delete();
                    else
                        note.chart.Remove(chordNote);
                }
            }
        }
    }
    
    protected override void UpdateCheck()
    {
        if (note != null)
        {
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

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            const float offset = 0.25f;
            
            if (Globals.bot)
            {
                hit = true;
                sustainBroken = false;
            }

            if (hit && transform.position.y <= editor.visibleStrikeline.position.y + offset)
            {
                if (isActivated)
                {
                    DeactivateNote();
                }

                // Resize sustain
                if (!sustainBroken && note.sustain_length > 0)
                {
                    float sustainEndPoint = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length);
                    float yPos = (sustainEndPoint + editor.visibleStrikeline.position.y) / 2;
                    float yScale = sustainEndPoint - (editor.visibleStrikeline.position.y);

                    if (yPos > editor.visibleStrikeline.position.y && yScale > 0)
                    {
                        sustain.transform.position = new Vector3(sustain.transform.position.x, yPos, sustain.transform.position.z);
                        sustain.transform.localScale = new Vector3(sustain.transform.localScale.x, yScale, sustain.transform.localScale.z);
                    }
                    else
                        sustainBroken = true;
                }
            }

            if (sustainBroken)
                sustainRen.enabled = false;
        }
    }

    public override void UpdateSongObject()
    {
        // Guard to prevent forcing errors
        if (note.CannotBeForcedCheck)
            note.flags &= ~Note.Flags.FORCED;

        if (note.song != null)
        {
            // Position
            if (note.fret_type != Note.Fret_Type.OPEN)
                transform.position = new Vector3(CHART_CENTER_POS + (int)note.fret_type - 2, note.worldYPosition, 0);
            else
                transform.position = new Vector3(CHART_CENTER_POS, note.worldYPosition, 0);

            // Note Type
            if (Globals.viewMode == Globals.ViewMode.Chart)
            {
                noteType = note.type;
            }
            else
            {
                // Do this simply because the HOPO glow by itself looks pretty cool
                noteType = Note.Note_Type.HOPO;
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
            noteRenderer.sortingOrder = -Mathf.Abs((int)note.position);
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
            // Visuals
            // Update mesh
            if (note.fret_type == Note.Fret_Type.OPEN)
                meshFilter.sharedMesh = Globals.openModel.sharedMesh;
            else if (specialType == Note.Special_Type.STAR_POW)
                meshFilter.sharedMesh = Globals.spModel.sharedMesh;
            else
                meshFilter.sharedMesh = Globals.standardModel.sharedMesh;
 
            Material[] materials;

            // Determine materials
            if (note.fret_type == Note.Fret_Type.OPEN)
            {
                materials = Globals.openRenderer.sharedMaterials;

                if (specialType == Note.Special_Type.STAR_POW)
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = Globals.openMaterials[3];
                    else
                        materials[2] = Globals.openMaterials[2];
                }
                else
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = Globals.openMaterials[1];
                    else
                        materials[2] = Globals.openMaterials[0];
                }
            }
            else
            {
                const int standardColMatPos = 1;
                const int spColMatPos = 3;

                switch (noteType)
                {
                    case (Note.Note_Type.HOPO):
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = Globals.spHopoRenderer.sharedMaterials;
                            materials[spColMatPos] = Globals.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = Globals.hopoRenderer.sharedMaterials;
                            materials[standardColMatPos] = Globals.strumColors[(int)note.fret_type];
                        }
                        break;
                    case (Note.Note_Type.TAP):
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = Globals.spTapRenderer.sharedMaterials;
                            materials[spColMatPos] = Globals.tapColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = Globals.tapRenderer.sharedMaterials;
                            materials[standardColMatPos] = Globals.tapColors[(int)note.fret_type];
                        }
                        break;
                    default:    // strum
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = Globals.spStrumRenderer.sharedMaterials;
                            materials[spColMatPos] = Globals.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = Globals.strumRenderer.sharedMaterials;
                            materials[standardColMatPos] = Globals.strumColors[(int)note.fret_type];
                        }
                        break;
                }
            }
            noteRenderer.sharedMaterials = materials;
#endif

            sustain.UpdateSustain();
        }
    }

    public void Activate()
    {
        noteRenderer.enabled = true;
        noteHitCollider.enabled = true;
        sustainRen.enabled = true;
        hit = false;
        sustainBroken = false;
        
    }

    void DeactivateNote()
    {
        noteRenderer.enabled = false;
        noteHitCollider.enabled = false;
    }

    public void HideFullNote()
    {
        DeactivateNote();
        hit = true;
        sustainBroken = true;
        sustainRen.enabled = false;
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

    public override void Delete(bool update = true)
    {
        note.chart.Remove(note, update);

        // Update the previous note in the case of chords with 2 notes
        if (note.previous != null)
            note.previous.controller.UpdateSongObject();
        if (note.next != null)
            note.next.controller.UpdateSongObject();

        Destroy(gameObject);
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
