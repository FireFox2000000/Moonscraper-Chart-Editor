using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimingConfig;

public class HitWindowFeeder : MonoBehaviour {

    [HideInInspector]
    public HitWindow<GuitarNoteHitKnowledge> guitarHitWindow = new HitWindow<GuitarNoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
    [HideInInspector]
    public HitWindow<DrumsNoteHitKnowledge> drumsHitWindow = new HitWindow<DrumsNoteHitKnowledge>(DrumsTiming.frontendHitWindowTime, DrumsTiming.backendHitWindowTime);
    List<NoteController> physicsWindow = new List<NoteController>();

    void OnTriggerEnter2D(Collider2D col)
    {
        NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
        if (nCon && !nCon.hit && !physicsWindow.Contains(nCon))
        {
            // We only want 1 note per position so that we can compare using the note mask
            foreach (NoteController insertedNCon in physicsWindow)
            {
                if (nCon.note.tick == insertedNCon.note.tick)
                    return;
            }

            // Insert into sorted position
            for (int i = 0; i < physicsWindow.Count; ++i)
            {
                if (nCon.note < physicsWindow[i].note)
                {
                    physicsWindow.Insert(i, nCon);
                    return;
                }
            }

            physicsWindow.Add(nCon);
        }
    }

    void Update()
    {
        float time = ChartEditor.Instance.currentVisibleTime;

        ChartEditor editor = ChartEditor.Instance;
        Chart.GameMode gameMode = editor.currentChart.gameMode;

        // Enter window
        for (int i = physicsWindow.Count - 1; i >= 0; --i)
        {
            NoteController note = physicsWindow[i];
            if (gameMode == Chart.GameMode.Guitar)
            {
                if (guitarHitWindow.DetectEnter(note.note, time))
                {
                    physicsWindow.Remove(note);
                }
            }
            else if (gameMode == Chart.GameMode.Drums)
            {
                if (drumsHitWindow.DetectEnter(note.note, time))
                {
                    physicsWindow.Remove(note);
                }
            }
        }
    }

    public void Reset()
    {
        guitarHitWindow.noteKnowledgeQueue.Clear();
        drumsHitWindow.noteKnowledgeQueue.Clear();
        physicsWindow.Clear();
    }
}
