//#define NOTE_TYPE_2D

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NoteController : SongObjectController {
    public const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    public const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public Note note { get { return (Note)songObject; } set { Init(value); } }
    public SustainController sustain;
    public GameObject noteVisuals;
    Whammy whammy;  

    Renderer noteRenderer;
    EdgeCollider2D noteHitCollider;

    protected Renderer sustainRen;
    BoxCollider2D sustainHitBox;
    BoxCollider hitBox;

    [HideInInspector]
    public bool hit = false;
    [HideInInspector]
    public bool sustainBroken = false;
    public bool isActivated
    {
        get
        {
            if (noteVisuals.gameObject.activeSelf)
                return true;
            else
                return false;
        }
    }
    new void Awake()
    {
        base.Awake();
        sustainRen = sustain.GetComponent<Renderer>();
        whammy = sustainRen.GetComponent<Whammy>();
    }

    public override void OnSelectableMouseDown()
    {
        if (Toolpane.currentTool == Toolpane.Tools.Cursor && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            // Ctrl-clicking
            if (Globals.modifierInputActive)
            {
                if (editor.IsSelected(songObject))
                    editor.RemoveFromSelectedObjects(songObject);
                else
                    editor.AddToSelectedObjects(songObject);
            }
            // Regular clicking
            else if (!editor.IsSelected(songObject))
            {/*
                if (Globals.secondaryInputActive && editor.currentSelectedObjects.Length > 0)
                {

                }
                else*/
                if (Input.GetButton("ChordSelect"))
                {
                    editor.currentSelectedObjects = note.GetChord();
                }
                else
                    editor.currentSelectedObject = songObject;
            }
        }

        // Delete the object on erase tool
        if ((Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor) ||
            (Input.GetMouseButtonDown(0) && Globals.applicationMode == Globals.ApplicationMode.Editor && Input.GetMouseButton(1)))
        {
            if (Input.GetButton("ChordSelect"))
            {
                if (Toolpane.currentTool == Toolpane.Tools.Eraser)
                    Debug.Log("Deleted " + note + " chord at position " + note.position + " with eraser tool");
                else
                    Debug.Log("Deleted " + note + " chord at position " + note.position + " with hold-right left-click shortcut");

                Note[] chordNotes = note.GetChord();
                editor.actionHistory.Insert(new ActionHistory.Delete(chordNotes));
                foreach (Note chordNote in chordNotes)
                {
                    chordNote.Delete();
                }
            }
            else
            {
                if (Toolpane.currentTool == Toolpane.Tools.Eraser)
                    Debug.Log("Deleted " + note + " at position " + note.position + " with eraser tool");
                else
                    Debug.Log("Deleted " + note + " at position " + note.position + " with hold-right left-click shortcut");

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
                editor.groupMove.SetSongObjects(chordNotes, 0);

                foreach (Note chordNote in chordNotes)
                    chordNote.Delete();
            }
            else
            {
                //base.OnSelectableMouseDrag();
            }
        }
        else
        {
            sustain.OnSelectableMouseDrag();
        }
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
            if (sustainHitBox)
                sustainHitBox.size = new Vector2(OPEN_NOTE_COLLIDER_WIDTH, sustainHitBox.size.y);

            // Change line renderer to fit open note
            if (whammy)
            {
                whammy.widthMultiplier = 0.25f;
                whammy.SetWidth(4);
            }

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
            // CHange line renderer to standard note
            if (whammy)
            {
                whammy.widthMultiplier = 1;
                whammy.SetWidth(1);
            }

            if (sustainHitBox)
                sustainHitBox.size = new Vector2(1, sustainHitBox.size.y);

            if (hitBox)
                hitBox.size = new Vector3(1, hitBox.size.y, hitBox.size.z);
        }

        UpdateSongObject();
    }

    public bool belowClapLine { get { return (transform.position.y <= editor.visibleStrikeline.position.y + (Song.TimeToWorldYPosition(Globals.clapCalibrationMS / 1000.0f))); } }
    public bool belowStrikeLine { get { const float offset = 0.02f; return (transform.position.y <= editor.visibleStrikeline.position.y + (offset * Globals.hyperspeed / Globals.gameSpeed)); } }

    protected override void UpdateCheck()
    {
        if (note != null)
        {
            uint endPosition = note.position + note.sustain_length;
            
            if ((note.position >= editor.minPos && note.position < editor.maxPos) ||
                    (endPosition > editor.minPos && endPosition < editor.maxPos) ||
                    (note.position < editor.minPos && endPosition >= editor.maxPos))
            {
                //if (Globals.applicationMode == Globals.ApplicationMode.Editor)
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
                if (Globals.bot && belowClapLine)
                {
                    if (!hit)
                    {
                        bool playClap = true;

                        switch (note.type)
                        {
                            case (Note.Note_Type.STRUM):
                                if ((Globals.clapSetting & Globals.ClapToggle.STRUM) == 0)
                                    playClap = false;
                                break;
                            case (Note.Note_Type.HOPO):
                                if ((Globals.clapSetting & Globals.ClapToggle.HOPO) == 0)
                                    playClap = false;
                                break;
                            case (Note.Note_Type.TAP):
                                if ((Globals.clapSetting & Globals.ClapToggle.TAP) == 0)
                                    playClap = false;
                                break;
                            default:
                                break;
                        }

                        if (playClap)
                            StrikelineAudioController.Clap(transform.position.y);
                    }

                    hit = true;
                    sustainBroken = false;
                }

                if (hit && belowStrikeLine)
                {
                    if (isActivated)
                    {
                        if (Globals.bot)
                        {
                            PlayIndicatorAnim();
                        }
                        DeactivateNote();
                    }
                    
                    // Resize sustain
                    if (!sustainBroken && note.sustain_length > 0)
                    {
                        float? prevSustainHeight = null;

                        float sustainEndPoint = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length);
                        if (sustainEndPoint > editor.camYMax.position.y)
                            sustainEndPoint = editor.camYMax.position.y;
                        else
                            prevSustainHeight = sustain.transform.localScale.y;

                        float yPos = (sustainEndPoint + editor.visibleStrikeline.position.y) / 2 + 0.3f;        // Added offset
                        float yScale = sustainEndPoint - (editor.visibleStrikeline.position.y);

                        if (yPos > editor.visibleStrikeline.position.y && yScale > 0)
                        {
                            sustain.transform.position = new Vector3(sustain.transform.position.x, yPos, sustain.transform.position.z);
                            sustain.transform.localScale = new Vector3(sustain.transform.localScale.x, yScale, sustain.transform.localScale.z);
                         
                            PlayIndicatorAnim();
                        }
                        else
                            sustainBroken = true;
                    }
                }

                if (sustainBroken)
                    sustainRen.enabled = false;

                if (whammy)
                {
                    if (hit && !sustainBroken && !Globals.bot)
                        whammy.canWhammy = true;
                    else
                        whammy.canWhammy = false;
                }
            }
            else if(whammy)
                whammy.canWhammy = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public static float GetXPos(float chartPos, Note note)
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
        {
            if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                return -chartPos + (int)note.fret_type + 2;
            else
                return chartPos + (int)note.fret_type - 2;

        }
        else
            return chartPos;
    }

    public static float noteToXPos(Note note)
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
        {
            if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
                return -(int)note.fret_type + 2;
            else
                return (int)note.fret_type - 2;
        }
        else
            return 0;
    }

    public override void UpdateSongObject()
    {
        if (note.song != null)
        {
            // Position
            transform.position = new Vector3(CHART_CENTER_POS + noteToXPos(note), note.worldYPosition, 0);

            if (!(note.sustain_length == 0 && sustain.transform.localScale.y == 0))
                sustain.UpdateSustain();
        }
    }

    public void Activate()
    {
        noteVisuals.gameObject.SetActive(true);
        sustainRen.enabled = true;
        hit = false;
        sustainBroken = false;
    }

    public void DeactivateNote()
    {
        noteVisuals.gameObject.SetActive(false);
    }

    public void PlayIndicatorAnim()
    {
        if (note.fret_type != Note.Fret_Type.OPEN)
        {
            editor.indicators.animations[(int)note.fret_type].PlayOneShot();
        }
        else
        {
            foreach (HitAnimation hitAnimation in editor.indicators.animations)
            {
                hitAnimation.PlayOneShot();
            }
        }
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
