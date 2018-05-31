using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWindowManager : MonoBehaviour {

    const float BACKEND_HIT_WINDOW_TIME = 0.2f;
    const float FRONTEND_HIT_WINDOW_TIME = 0.116f;

    [HideInInspector]
    public HitWindow hitWindow = new HitWindow(FRONTEND_HIT_WINDOW_TIME, BACKEND_HIT_WINDOW_TIME);
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

    // Returns true if hit notes have been exited at the notestreak should be reset
    public int UpdateHitWindow(uint noteStreak)
    {
        float time = TickFunctions.WorldYPositionToTime(ChartEditor.GetInstance().visibleStrikeline.position.y);

        // Enter window
        foreach (NoteController note in physicsWindow.ToArray())
        {
            if (hitWindow.DetectEnter(note.note, time))
            {
                physicsWindow.Remove(note);
            }
        }

        // Exit window
        {
            var notesRemoved = hitWindow.DetectExit(time);
            int missCount = 0;
            foreach (var noteKnowledge in notesRemoved)
            {
                // Miss, exited window
                if (!noteKnowledge.hasBeenHit)
                {
                    foreach (Note chordNote in noteKnowledge.note.GetChord())
                    {
                        chordNote.controller.sustainBroken = true;

                        if (noteStreak > 0)
                            chordNote.controller.DeactivateNote();
                    }

                    ++missCount;
                }
            }

            return missCount;
        }
    }

    public void Reset()
    {
        hitWindow.noteKnowledge.Clear();
        physicsWindow.Clear();
    }
}
