// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

//#define NOTE_TYPE_2D

using UnityEngine;
using MoonscraperChartEditor.Song;

public class NoteController : SongObjectController {
    public const float OPEN_NOTE_SUSTAIN_WIDTH = 4;
    public const float OPEN_NOTE_COLLIDER_WIDTH = 5;

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

    bool _hit = false;
    [HideInInspector]
    public bool hit
    {
        get
        {
            return _hit;
        }
        set
        {
            _hit = value;
            whammy.enabled = _hit;
        }
    }
    [HideInInspector]
    public bool sustainBroken = false;
    public bool isActivated
    {
        get
        {
            return noteVisuals.gameObject.activeSelf;
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
        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor && editor.currentState == ChartEditor.State.Editor && Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            var selectedObjectsManager = editor.selectedObjectsManager;

            // Ctrl-clicking
            if (Globals.modifierInputActive)
            {
                if (selectedObjectsManager.IsSelected(songObject))
                    selectedObjectsManager.RemoveFromSelectedObjects(songObject);
                else
                    selectedObjectsManager.AddToSelectedObjects(songObject);

                return;
            }

            // Shift-clicking
            if (Globals.secondaryInputActive)
            {
                int pos = SongObjectHelper.FindClosestPosition(this.songObject, editor.selectedObjectsManager.currentSelectedObjects);

                if (pos != SongObjectHelper.NOTFOUND)
                {
                    uint min;
                    uint max;

                    if (editor.selectedObjectsManager.currentSelectedObjects[pos].tick > songObject.tick)
                    {
                        max = editor.selectedObjectsManager.currentSelectedObjects[pos].tick;
                        min = songObject.tick;
                    }
                    else
                    {
                        min = editor.selectedObjectsManager.currentSelectedObjects[pos].tick;
                        max = songObject.tick;
                    }

                    var chartObjects = editor.currentChart.chartObjects;
                    int index, length;
                    SongObjectHelper.GetRange(chartObjects, min, max, out index, out length);
                    selectedObjectsManager.SetCurrentSelectedObjects(chartObjects, index, length);

                    return;
                }
            }

            // Regular clicking
            if (!selectedObjectsManager.IsSelected(songObject))
            {
                if (MSChartEditorInput.GetInput(MSChartEditorInputActions.ChordSelect))
                    selectedObjectsManager.SetCurrentSelectedObjects(note.chord);
                else
                    selectedObjectsManager.currentSelectedObject = songObject;

                return;
            }
        }

        // Delete the object on left and right click shortcut
        else if (editor.currentState == ChartEditor.State.Editor &&
            Input.GetMouseButtonDown(0) && Input.GetMouseButton(1))
        {
            if (MSChartEditorInput.GetInput(MSChartEditorInputActions.ChordSelect))
            {
                Note[] chordNotes = note.GetChord();
                if (Input.GetMouseButton(1))
                {
                    if (SustainController.SustainDraggingInProgress)
                        editor.commandStack.Pop();  // Cancel the last sustain drag action

                    Debug.Log("Deleted " + note + " chord at position " + note.tick + " with hold-right left-click shortcut");
                    editor.commandStack.Push(new SongEditDelete(chordNotes));

                    SustainController.ResetSustainDragData();
                }
            }
            else if (Input.GetMouseButton(1))
            {
                if (SustainController.SustainDraggingInProgress)
                    editor.commandStack.Pop();    // Cancel the last sustain drag action

                Debug.Log("Deleted " + note + " at position " + note.tick + " with hold-right left-click shortcut");
                editor.commandStack.Push(new SongEditDelete(note));
                SustainController.ResetSustainDragData();
            }
        }
        else
        {
            sustain.OnSelectableMouseDown();
        }
    }

    public override void OnSelectableMouseDrag()
    {
        if (!moveCheck)
        {
            sustain.OnSelectableMouseDrag();
        }
    }

    public override void OnSelectableMouseUp()
    {
        sustain.OnSelectableMouseUp();
    }

    void Init(Note note)
    {
        base.Init(note, this);

        if (note == null)
            return;

        sustain.gameObject.SetActive(note.length != 0);

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
        transform.position = new Vector3(CHART_CENTER_POS + NoteToXPos(note), desiredWorldYPosition, zPos);
    }

    protected override void UpdateCheck()
    {
        Note note = this.note;
        if (note != null)
        {         
            uint endPosition = note.tick + note.length;
            
            // Determine if a note is outside of the view range
            if (endPosition < editor.minPos || note.tick > editor.maxPos)
            {
                gameObject.SetActive(false);
                return;
            }

            if (isDirty)
                UpdateSongObject();

            if (note.tick > editor.maxPos)
                gameObject.SetActive(false);

            if (this.note == null)      // Was deactivated
                return;

           sustain.gameObject.SetActive(note.length != 0 || Input.GetMouseButton(1));

            // Sustain is constantly updated unless it has no length or it's length is meant to be zero but isn't
            if (!(note.length == 0 && sustain.transform.localScale.y == 0))
                sustain.UpdateSustain();

            // Handle gameplay operation
            if (editor.currentState == ChartEditor.State.Playing)
            {
                ManageGameplay();
            }
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

        bool belowStrikeLine = notePosition.y <= strikelinePosition.y + (Time.deltaTime / Globals.gameSettings.hyperspeed * Globals.gameSettings.gameSpeed);

        if (hit && belowStrikeLine)
        {
            if (isActivated)
            {
                DeactivateNote();
            }

            // Resize sustain
            if (!sustainBroken && note.length > 0)
            {
                GameplaySustainHold();
            }
        }

        if (sustainBroken)
            sustainRen.enabled = false;
    }

    public void SetDesiredWhammy(float whammyBarValue)
    {
        whammy.desiredWhammy = whammyBarValue;
    }

    void GameplaySustainHold()
    {
        float sustainEndPoint = note.song.TickToWorldYPosition(note.tick + note.length);
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
            bool leftyFlip = Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip;

            return chartPos + ChartEditor.Instance.laneInfo.GetLanePosition(note.rawNote, leftyFlip);
        }
        else
            return chartPos;
    }

    public static float NoteToXPos(Note note)
    {
        return GetXPos(0, note);
    }

    public override void UpdateSongObject()
    {
        Note note = this.note;
        if (note.song != null)
        {
            float zPos = 0;
            // Position
            transform.position = new Vector3(CHART_CENTER_POS + NoteToXPos(note), desiredWorldYPosition, zPos);

            if (note.IsOpenNote())
                sustainRen.sortingOrder = -1;
            else
                sustainRen.sortingOrder = 0;

            noteObjectSelector.UpdateSelectedGameObject();

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
                editor.indicators.animations[note.GetRawNoteLaneCapped(ChartEditor.Instance.laneInfo)].PlayOneShot();
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
        if (previousNote == null || previousNote.tick != openNotePos || (!previousNote.isChord && previousNote.tick != openNotePos))
            return previousNote;
        else
            return GetPreviousOfOpen(openNotePos, previousNote.previous);
    }

    static Note GetNextOfOpen(uint openNotePos, Note nextNote)
    {
        if (nextNote == null || nextNote.tick != openNotePos || (!nextNote.isChord && nextNote.tick != openNotePos))
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
