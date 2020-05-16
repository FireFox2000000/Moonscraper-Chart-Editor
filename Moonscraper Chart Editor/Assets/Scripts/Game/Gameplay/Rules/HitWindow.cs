// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitWindow
{
    bool DetectEnter(Note note, float time);
    void Clear();
}

public class HitWindow<TNoteHitKnowledge> : IHitWindow where TNoteHitKnowledge : NoteHitKnowledge
{
    float frontendTime;
    float backendTime;

    private List<TNoteHitKnowledge> m_noteQueue;

    public List<TNoteHitKnowledge> noteKnowledgeQueue
    {
        get
        {
            return m_noteQueue;
        }
    }

    public TNoteHitKnowledge oldestUnhitNote
    {
        get
        {
            // New notes inserted at the end
            for (int i = 0; i < noteKnowledgeQueue.Count; ++i)
            {
                if (!noteKnowledgeQueue[i].hasBeenHit)
                    return noteKnowledgeQueue[i];
            }
            return null;
        }
    }

    public HitWindow(float frontendTime, float backendTime)
    {
        m_noteQueue = new List<TNoteHitKnowledge>();
        this.frontendTime = frontendTime;
        this.backendTime = backendTime;
    }

    public bool DetectEnter(Note note, float time)
    {
        if (note.time > time + frontendTime)
            return false;

        foreach (TNoteHitKnowledge noteHitData in m_noteQueue)
        {
            if (note.tick == noteHitData.note.tick)
                return false;
        }

        TNoteHitKnowledge newNoteKnowledge = System.Activator.CreateInstance(typeof(TNoteHitKnowledge), note) as TNoteHitKnowledge;
        m_noteQueue.Add(newNoteKnowledge);
   
        if (m_noteQueue.Count > 1) 
        {
            uint tick1 = m_noteQueue[m_noteQueue.Count - 1].note.tick;
            uint tick2 = m_noteQueue[m_noteQueue.Count - 2].note.tick;

            Debug.Assert(tick1 > tick2, string.Format("Notes inserted into hit window in the wrong order, tick1 = {0}, tick2 = {1}", tick1, tick2));
        }


        return true;
    }

    public void Clear()
    {
        noteKnowledgeQueue.Clear();
    }

    public List<TNoteHitKnowledge> DetectExit(float time)
    {
        List<TNoteHitKnowledge> elementsRemoved = new List<TNoteHitKnowledge>();

        for (int i = m_noteQueue.Count - 1; i >= 0; --i)
        {
            TNoteHitKnowledge noteKnowledge = m_noteQueue[i];
            Note note = noteKnowledge.note;
            Note nextNote = GetNextNote(i);

            bool fallenOutOfWindow = !IsWithinTimeWindow(note, nextNote, time);

            if (fallenOutOfWindow || noteKnowledge.shouldExitWindow)
            {
                elementsRemoved.Add(m_noteQueue[i]);
                m_noteQueue.RemoveAt(i);
            }
        }

        return elementsRemoved;
    }

    public int GetPosition(Note note)
    {
        for (int i = 0; i < m_noteQueue.Count; ++i)
        {
            TNoteHitKnowledge noteHitData = m_noteQueue[i];
            if (note == noteHitData.note)
                return i;
        }

        return -1;
    }

    public TNoteHitKnowledge Get(Note note, float time)
    {
        int index = GetPosition(note);

        if (index >= 0)
        {
            TNoteHitKnowledge noteData = m_noteQueue[index];
            Note nextNote = GetNextNote(index);

            if (IsWithinTimeWindow(note, nextNote, time))
                return noteData;
        }

        return null;
    }

    public bool Contains(Note note, float time)
    {
        return Get(note, time) != null;
    }

    // Must have a note to test. Next note is optional.
    public bool IsWithinTimeWindow(Note note, Note nextNote, float time)
    {
        return !(note.time < time - backendTime                                                         // Out of window range
                || (nextNote != null && note.time <= time - (nextNote.time - note.time) / 2.0f));        // Half the distance to the next note
    }

    Note GetNextNote(int index)
    {
        return index < m_noteQueue.Count - 1 ? m_noteQueue[index + 1].note : null;
    }
}
