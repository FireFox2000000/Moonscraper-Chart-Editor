// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#define NOTE_TYPE_2D

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NoteController : SongObjectController {
    public const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    public const float OPEN_NOTE_COLLIDER_WIDTH = 5;

    public static float positionIncrementFactor { get { return Globals.ghLiveMode ? 0.8f : 1.0f; } }
    public static float noteObjectPositionStartOffset = -2;

    public Note note { get { return (Note)songObject; } set { Init(value); } }
    public SustainController sustain;
    public GameObject noteVisuals;
    public Note2D3DSelector noteObjectSelector;
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
            // Shift-clicking
            else if (Globals.secondaryInputActive)
            {
                int pos = SongObject.FindClosestPosition(this.songObject, editor.currentSelectedObjects);

                if (pos != SongObject.NOTFOUND)
                {
                    uint min;
                    uint max;

                    if (editor.currentSelectedObjects[pos].position > songObject.position)
                    {
                        max = editor.currentSelectedObjects[pos].position;
                        min = songObject.position;
                    }
                    else
                    {
                        min = editor.currentSelectedObjects[pos].position;
                        max = songObject.position;
                    }

                    editor.currentSelectedObjects = SongObject.GetRangeCopy(editor.currentChart.chartObjects, min, max);
                }
            }
            // Regular clicking
            else if (!editor.IsSelected(songObject))
            {
                if (Input.GetButton("ChordSelect"))
                    editor.currentSelectedObjects = note.GetChord();
                else
                    editor.currentSelectedObject = songObject;
            }
        }

        // Delete the object on erase tool
        else if (Globals.applicationMode == Globals.ApplicationMode.Editor &&
            (
            (Toolpane.currentTool == Toolpane.Tools.Eraser && Input.GetMouseButtonDown(0)) ||
            (Input.GetMouseButtonDown(0) && Input.GetMouseButton(1)) ||
            Eraser.dragging)
            )
        {
            if (Input.GetButton("ChordSelect"))
            {
                Note[] chordNotes = note.GetChord();

                if (!Input.GetMouseButton(1))
                {
                    Debug.Log("Deleted " + note + " chord at position " + note.position + " with eraser tool");
                    Eraser.dragEraseHistory.Add(new ActionHistory.Delete(chordNotes));
                }
                else
                {
                    Debug.Log("Deleted " + note + " chord at position " + note.position + " with hold-right left-click shortcut");
                    editor.actionHistory.Insert(new ActionHistory.Delete(chordNotes));
                }

                
                //editor.actionHistory.Insert(new ActionHistory.Delete(chordNotes));
                foreach (Note chordNote in chordNotes)
                {
                    chordNote.Delete();
                }
            }
            else
            {
                if (!Input.GetMouseButton(1))
                {
                    Debug.Log("Deleted " + note + " at position " + note.position + " with eraser tool");
                    Eraser.dragEraseHistory.Add(new ActionHistory.Delete(note));
                }
                else
                {
                    Debug.Log("Deleted " + note + " at position " + note.position + " with hold-right left-click shortcut");
                    editor.actionHistory.Insert(new ActionHistory.Delete(note));
                }

                note.Delete();
            }
        }
    }
    
    public override void OnSelectableMouseOver()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor && Eraser.dragging)
        {
            OnSelectableMouseDown();
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

        sustain.gameObject.SetActive(note.sustain_length != 0);

        if (!hitBox)
            hitBox = GetComponent<BoxCollider>();
        if (!sustainHitBox)
            sustainHitBox = sustain.GetComponent<BoxCollider2D>();

        if (note.IsOpenNote())
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
            if (hitBox)
                hitBox.size = new Vector3(OPEN_NOTE_COLLIDER_WIDTH, hitBox.size.y, hitBox.size.z);
            
            if (!Globals.drumMode)
            {
                Note[] chordNotes = note.GetChord();

                // Check for non-open notes and delete
                foreach (Note chordNote in chordNotes)
                {
                    if (!chordNote.IsOpenNote())
                    {
                        chordNote.Delete();
                    }
                }
            }
        }
        else
        {
            // Change line renderer to standard note
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
        //UpdateNotePosition();
    }

    void UpdateNotePosition()
    {
        float zPos = 0;
        // Position
        transform.position = new Vector3(CHART_CENTER_POS + NoteToXPos(note), note.worldYPosition, zPos);
    }

    protected override void UpdateCheck()
    {
        Note note = this.note;
        if (note != null)
        {         
            uint endPosition = note.position + note.sustain_length;

            // Determine if a note is outside of the view range
            if (endPosition < editor.minPos)
            {
                gameObject.SetActive(false);
                return;
            }

            if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            {
                if (isDirty)
                    UpdateSongObject();

                if (note.position > editor.maxPos)
                    gameObject.SetActive(false);
                //else if (Globals.viewMode == Globals.ViewMode.Chart)
                    //UpdateSongObject();         // Always update the position in case of hyperspeed changes
               // else
                   // UpdateNotePosition();
            }

            if (this.note == null)      // Was deactivated
                return;

           //if (Input.GetMouseButton(1) && !sustain.gameObject.activeSelf)
           sustain.gameObject.SetActive(note.sustain_length != 0 || Input.GetMouseButton(1));

            // Sustain is constantly updated unless it has no length or it's length is meant to be zero but isn't
            if (!(note.sustain_length == 0 && sustain.transform.localScale.y == 0))
                sustain.UpdateSustain();

            // Handle gameplay operation
            if (Globals.applicationMode == Globals.ApplicationMode.Playing)
            {
                ManageGameplay();
            }
            else if(whammy)
                whammy.canWhammy = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void ManageGameplay()
    {
        Vector3 notePosition = transform.position;
        Vector3 strikelinePosition = editor.visibleStrikeline.position;

        bool belowClapLine = notePosition.y <= strikelinePosition.y + (Song.TimeToWorldYPosition(Globals.audioCalibrationMS / 1000.0f) * Globals.gameSpeed);
        bool belowStrikeLine = notePosition.y <= strikelinePosition.y + (Time.deltaTime * Globals.hyperspeed / Globals.gameSpeed);

        if (Globals.bot && belowClapLine)
        {
            GameplayBotHitClap();
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
                GameplaySustainHold();
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

    void GameplayBotHitClap()
    {
        if (!hit)
        {
            bool playClap = true;

            switch (note.type)
            {
                case (Note.Note_Type.Strum):
                    if ((Globals.clapSetting & Globals.ClapToggle.STRUM) == 0)
                        playClap = false;
                    break;
                case (Note.Note_Type.Hopo):
                    if ((Globals.clapSetting & Globals.ClapToggle.HOPO) == 0)
                        playClap = false;
                    break;
                case (Note.Note_Type.Tap):
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

    void GameplaySustainHold()
    {
        float sustainEndPoint = note.song.ChartPositionToWorldYPosition(note.position + note.sustain_length);
        if (sustainEndPoint > editor.camYMax.position.y)
            sustainEndPoint = editor.camYMax.position.y;

        float yPos = (sustainEndPoint + editor.visibleStrikeline.position.y) / 2;
        float yScale = sustainEndPoint - (editor.visibleStrikeline.position.y);
        const float OFFSET = 0.1f;

        if (yPos > editor.visibleStrikeline.position.y && yScale > 0)
        {
            sustain.transform.position = new Vector3(sustain.transform.position.x, yPos + OFFSET, sustain.transform.position.z);
            sustain.transform.localScale = new Vector3(sustain.transform.localScale.x, yScale - (2 * OFFSET), sustain.transform.localScale.z);

            PlayIndicatorAnim();
        }
        else
            sustainBroken = true;
    }

    public static float GetXPos(float chartPos, Note note)
    {
        if (!note.IsOpenNote())
        {
            if (Globals.notePlacementMode == Globals.NotePlacementMode.LeftyFlip)
            {
                return chartPos - note.rawNote * positionIncrementFactor - noteObjectPositionStartOffset;
            }
            else
                return chartPos + note.rawNote * positionIncrementFactor + noteObjectPositionStartOffset;

        }
        else
            return chartPos;
    }

    public static float NoteToXPos(Note note)
    {
        return GetXPos(0, note);
        /*
        if (!note.IsOpenNote())
        {
            if (Globals.notePlacementMode != Globals.NotePlacementMode.LeftyFlip)
            {
                return -note.rawNote + positionToOffsetInDisplay;
            }
            else
                return note.rawNote - positionToOffsetInDisplay;
        }
        else
            return 0;*/
    }

    //Note.Fret_Type prevFretType;
   // Note.Flags prevFlags;

    public override void UpdateSongObject()
    {
        Note note = this.note;
        if (note.song != null)
        {
            float zPos = 0;
            // Position
            transform.position = new Vector3(CHART_CENTER_POS + NoteToXPos(note), note.worldYPosition, zPos);

            if (note.IsOpenNote())
                sustainRen.sortingOrder = -1;
            else
                sustainRen.sortingOrder = 0;

            if (noteObjectSelector.enabled)
                noteObjectSelector.UpdateSelectedGameObject();
            noteObjectSelector.currentVisualsManager.UpdateVisuals();

            UpdateNotePosition();
        }

        isDirty = false;
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
        if (!note.IsOpenNote())
        {
            if (note.rawNote < editor.indicators.animations.Length)
                editor.indicators.animations[note.rawNote].PlayOneShot();
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
