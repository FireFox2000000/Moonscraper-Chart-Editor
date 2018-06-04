using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWindowManager : MonoBehaviour {

    [HideInInspector]
    public HitWindow<GuitarNoteHitKnowledge> guitarHitWindow = new HitWindow<GuitarNoteHitKnowledge>(GuitarGameplayConfig.frontendHitWindowTime, GuitarGameplayConfig.backendHitWindowTime);
    [HideInInspector]
    public HitWindow<DrumsNoteHitKnowledge> drumsHitWindow = new HitWindow<DrumsNoteHitKnowledge>(GuitarGameplayConfig.frontendHitWindowTime, GuitarGameplayConfig.backendHitWindowTime);

    List<NoteController> physicsWindow = new List<NoteController>();

    void OnTriggerEnter2D(Collider2D col)
    {
        NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
        if (nCon && !nCon.hit && !physicsWindow.Contains(nCon))
        {
            // We only want 1 note per position so that we can compare using the note mask
            foreach (NoteController insertedNCon in physicsWindow)
            {
                if (nCon.note.position == insertedNCon.note.position)
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
        float time = ChartEditor.GetInstance().currentVisibleTime;

        // Enter window
        foreach (NoteController note in physicsWindow.ToArray())
        {
            ChartEditor editor = ChartEditor.GetInstance();
            Chart.GameMode gameMode = editor.currentChart.gameMode;

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
