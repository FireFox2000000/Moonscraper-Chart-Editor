﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimingConfig;

public class HitWindowFeeder : MonoBehaviour {

    [HideInInspector]
    public HitWindow<GuitarNoteHitKnowledge> hitWindow = new HitWindow<GuitarNoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
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
            if (hitWindow.DetectEnter(note.note, time))
            {
                physicsWindow.Remove(note);
            }
        }
    }

    public void Reset()
    {
        hitWindow.noteKnowledgeQueue.Clear();
        physicsWindow.Clear();
    }
}
