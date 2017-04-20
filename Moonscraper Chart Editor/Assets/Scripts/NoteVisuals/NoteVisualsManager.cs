using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisualsManager : MonoBehaviour {
    public NoteController nCon;
    protected Renderer noteRenderer;

    [HideInInspector]
    public Note.Note_Type noteType = Note.Note_Type.STRUM;
    [HideInInspector]
    public Note.Special_Type specialType = Note.Special_Type.NONE;

    Note prevNote;

    // Use this for initialization
    protected virtual void Awake () {
        noteRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        if (noteRenderer)
            UpdateVisuals();
    }

    void LateUpdate()
    {        
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)
            UpdateVisuals();

        Animate();
    }

    // Update is called once per frame
    public virtual void UpdateVisuals() {
        Note note = nCon.note;
        if (nCon.note != null)
        {
            noteType = GetTypeWithViewChange(note);

            // Star power?
            specialType = IsStarpower(note);

            // Update note visuals
            noteRenderer.sortingOrder = -Mathf.Abs((int)note.position);
        }
    }

    protected virtual void Animate() {}

    public static Note.Note_Type GetTypeWithViewChange(Note note)
    {
        if (Globals.viewMode == Globals.ViewMode.Chart)
        {
            return note.type;
        }
        else
        {
            // Do this simply because the HOPO glow by itself looks pretty cool
            return Note.Note_Type.HOPO;
        }
    }

    public static Note.Special_Type IsStarpower(Note note)
    {
        Note.Special_Type specialType = Note.Special_Type.NONE;

        foreach (Starpower sp in note.chart.starPower)
        {
            if (sp.position == note.position || (sp.position <= note.position && sp.position + sp.length > note.position))
            {
                specialType = Note.Special_Type.STAR_POW;
            }
            else if (sp.position > note.position)
                break;
        }

        return specialType;
    }
}
