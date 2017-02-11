//#define NOTE_TYPE_2D

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteController : SongObjectController {
    public const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    public const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public Note note { get { return (Note)songObject; } set { Init(value); } }
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
    BoxCollider2D sustainHitBox;
    BoxCollider hitBox;

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
                editor.actionHistory.Insert(new ActionHistory.Delete(chordNotes));
                foreach (Note chordNote in chordNotes)
                {
                    chordNote.Delete();
                }
            }
            else
            {
                editor.actionHistory.Insert(new ActionHistory.Delete(note));
                note.Delete();
            }
        }
    }

    public override void OnSelectableMouseDrag()
    {
        // Move note
        if (moveCheck)
        {
            if (Input.GetButton("ChordSelect"))
            {
                Note[] chordNotes = note.GetChord();
                editor.groupMove.SetSongObjects(chordNotes);

                foreach (Note chordNote in chordNotes)
                    chordNote.Delete();
            }
            else
            {
                base.OnSelectableMouseDrag();
            }
        }
        else
            sustain.OnSelectableMouseDrag();
    }

    public override void OnSelectableMouseUp()
    {
        sustain.OnSelectableMouseUp();
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
        nCon.note.Delete();

        return moveNoteController;
    }

    void Init(Note note)
    {
        base.Init(note, this);

        if (note == null)
            return;

        if (!hitBox)
            hitBox = GetComponent<BoxCollider>();
        if (!sustainHitBox)
            sustainHitBox = sustain.GetComponent<BoxCollider2D>();

        if (note.fret_type == Note.Fret_Type.OPEN)
        {
            // Apply scaling
            sustainRen.sprite = PrefabGlobals.openSustainSprite;
          
            if (sustainHitBox)
                sustainHitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, sustainHitBox.size.y);

            // Adjust note hitbox size
#if NOTE_TYPE_2D
            hitBox = GetComponent<BoxCollider2D>();
            if (hitBox)
                hitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y);
#else            
            if (hitBox)
                hitBox.size = new Vector3(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y, hitBox.size.z);
#endif
            Note[] chordNotes = note.GetChord();

            // Check for non-open notes and delete
            foreach (Note chordNote in chordNotes)
            {
                if (chordNote.fret_type != Note.Fret_Type.OPEN)
                {
                    chordNote.Delete();
                }
            }
        }
        else
        {
            if (sustainHitBox)
                sustainHitBox.size = new Vector2(1, sustainHitBox.size.y);

            if (hitBox)
                hitBox.size = new Vector3(1, hitBox.size.y, hitBox.size.z);
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
            {
                gameObject.SetActive(false);
                return;
            }

            // Handle gameplay operation
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
        else
        {
            gameObject.SetActive(false);
        }
    }

    public static float GetXPos(float chartPos, Note note)
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
            return chartPos + (int)note.fret_type - 2;
        else
            return chartPos;
    }

    public static float noteToXPos(Note note)
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
            return (int)note.fret_type - 2;
        else
            return 0;
    }

    public override void UpdateSongObject()
    {
        // Guard to prevent forcing errors
        //if (note.CannotBeForcedCheck)
            //note.flags &= ~Note.Flags.FORCED;

        if (note.song != null)
        {
            // Position
            transform.position = new Vector3(CHART_CENTER_POS + noteToXPos(note), note.worldYPosition, 0);

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
            foreach (Starpower sp in note.chart.starPower)
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
                meshFilter.sharedMesh = PrefabGlobals.openModel.sharedMesh;
            else if (specialType == Note.Special_Type.STAR_POW)
                meshFilter.sharedMesh = PrefabGlobals.spModel.sharedMesh;
            else
                meshFilter.sharedMesh = PrefabGlobals.standardModel.sharedMesh;
 
            Material[] materials;

            // Determine materials
            if (note.fret_type == Note.Fret_Type.OPEN)
            {
                materials = PrefabGlobals.openRenderer.sharedMaterials;

                if (specialType == Note.Special_Type.STAR_POW)
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = PrefabGlobals.openMaterials[3];
                    else
                        materials[2] = PrefabGlobals.openMaterials[2];
                }
                else
                {
                    if (noteType == Note.Note_Type.HOPO)
                        materials[2] = PrefabGlobals.openMaterials[1];
                    else
                        materials[2] = PrefabGlobals.openMaterials[0];
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
                            materials = PrefabGlobals.spHopoRenderer.sharedMaterials;
                            materials[spColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = PrefabGlobals.hopoRenderer.sharedMaterials;
                            materials[standardColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                        }
                        break;
                    case (Note.Note_Type.TAP):
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = PrefabGlobals.spTapRenderer.sharedMaterials;
                            materials[spColMatPos] = PrefabGlobals.tapColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = PrefabGlobals.tapRenderer.sharedMaterials;
                            materials[standardColMatPos] = PrefabGlobals.tapColors[(int)note.fret_type];
                        }
                        break;
                    default:    // strum
                        if (specialType == Note.Special_Type.STAR_POW)
                        {
                            materials = PrefabGlobals.spStrumRenderer.sharedMaterials;
                            materials[spColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
                        }
                        else
                        {
                            materials = PrefabGlobals.strumRenderer.sharedMaterials;
                            materials[standardColMatPos] = PrefabGlobals.strumColors[(int)note.fret_type];
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

    static Note GetPreviousOfOpen(uint openNotePos, Note previousNote)
    {
        if (previousNote == null || previousNote.position != openNotePos || (!previousNote.IsChord && previousNote.position != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    static Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.position != openNotePos || (!nextNote.IsChord && nextNote.position != openNotePos))
            return nextNote;
        else
            return GetNextOfOpen(openNotePos, nextNote.next);
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
