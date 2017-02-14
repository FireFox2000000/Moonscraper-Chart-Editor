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
    Globals.ApplicationMode prevMode;

    // Use this for initialization
    protected virtual void Start () {
        noteRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        if (noteRenderer)
            UpdateVisuals();
    }

    void Update()
    {
        UpdateVisuals();

        prevMode = Globals.applicationMode;
    }

    // Update is called once per frame
    protected virtual void UpdateVisuals() {
        Note note = nCon.note;
        if (nCon.note != null)
        {
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
        }
    }
}
