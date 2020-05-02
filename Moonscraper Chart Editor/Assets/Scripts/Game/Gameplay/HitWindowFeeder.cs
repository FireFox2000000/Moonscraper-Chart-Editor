// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimingConfig;

public class HitWindowFeeder : MonoBehaviour {
    List<NoteController> physicsWindow = new List<NoteController>();
    public IHitWindow hitWindow = null;

    float initSize;

    private void Awake()
    {
        initSize = transform.localScale.y;
        enabled = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        NoteController nCon = col.gameObject.GetComponentInParent<NoteController>();
        TryAddNote(nCon);
    }

    public void TryAddNote(NoteController nCon)
    {
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

        // Enter window, need to insert in the correct order
        for (int i = 0; i < physicsWindow.Count; ++i)
        {
            NoteController note = physicsWindow[i];

            if (!note.isActiveAndEnabled)
            {
                physicsWindow.Remove(note);
                --i;
            }
            else if (hitWindow != null)
            {
                if (hitWindow.DetectEnter(note.note, time))
                {
                    physicsWindow.Remove(note);
                    --i;
                }
            }
        }
    }

    private void OnEnable()
    {
        Reset();
        transform.localScale = new Vector3(transform.localScale.x, initSize, transform.localScale.z);
    }

    private void OnDisable()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        hitWindow = null;
    }

    public void Reset()
    {
        if (hitWindow != null)
        {
            hitWindow.Clear();
        }
        physicsWindow.Clear();
    }
}
