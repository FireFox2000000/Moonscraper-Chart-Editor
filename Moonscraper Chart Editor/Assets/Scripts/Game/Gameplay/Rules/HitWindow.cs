// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public interface IHitWindow
{
    bool DetectEnter(Note note, float time);
    void Clear();
}

public class HitWindow<TNoteHitKnowledge> : IHitWindow where TNoteHitKnowledge : NoteHitKnowledge
{
    float frontendTime;
    float backendTime;

    const int PoolCapacity = 100;

    private List<TNoteHitKnowledge> m_noteQueue;
    private List<TNoteHitKnowledge> m_noteHitKnowledgePool;

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
        m_noteHitKnowledgePool = new List<TNoteHitKnowledge>();
        this.frontendTime = frontendTime;
        this.backendTime = backendTime;

        m_noteQueue.Capacity = PoolCapacity;
        m_noteHitKnowledgePool.Capacity = PoolCapacity;
        PopulateHitKnowledgePool(PoolCapacity);
    }

    void PopulateHitKnowledgePool(int objectCount)
    {
        for (int i = 0; i < objectCount; ++i)
        {
            TNoteHitKnowledge newNoteKnowledge = System.Activator.CreateInstance(typeof(TNoteHitKnowledge)) as TNoteHitKnowledge;
            m_noteHitKnowledgePool.Add(newNoteKnowledge);
        }
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

        var newNoteKnowledge = PopNoteKnowledgeFromPool();
        newNoteKnowledge.SetFrom(note);
        m_noteQueue.Add(newNoteKnowledge);
   
        if (m_noteQueue.Count > 1) 
        {
            uint tick1 = m_noteQueue[m_noteQueue.Count - 1].note.tick;
            uint tick2 = m_noteQueue[m_noteQueue.Count - 2].note.tick;

            if (tick1 <= tick2)     // Branch so we avoid string concates showing up in the profiler
            {
                Debug.Assert(false, string.Format("Notes inserted into hit window in the wrong order, tick1 = {0}, tick2 = {1}", tick1, tick2));
            }
        }


        return true;
    }

    TNoteHitKnowledge PopNoteKnowledgeFromPool()
    {
        if (m_noteHitKnowledgePool.Count <= 0)
        {
            PopulateHitKnowledgePool(100);
        }

        var newNoteKnowledge = m_noteHitKnowledgePool[m_noteHitKnowledgePool.Count - 1]; m_noteHitKnowledgePool.RemoveAt(m_noteHitKnowledgePool.Count - 1);

        return newNoteKnowledge;
    }

    void ReturnNoteKnowledgeToPool(TNoteHitKnowledge knowledge)
    {
        m_noteHitKnowledgePool.Add(knowledge);
    }

    public void Clear()
    {
        noteKnowledgeQueue.Clear();
    }

    List<TNoteHitKnowledge> elementsRemoved = new List<TNoteHitKnowledge>();
    public List<TNoteHitKnowledge> DetectExit(float time)
    {
        elementsRemoved.Clear();

        for (int i = m_noteQueue.Count - 1; i >= 0; --i)
        {
            TNoteHitKnowledge noteKnowledge = m_noteQueue[i];
            Note note = noteKnowledge.note;
            Note nextNote = GetNextNote(i);

            bool fallenOutOfWindow = !IsWithinTimeWindow(note, nextNote, time);

            if (fallenOutOfWindow || noteKnowledge.shouldExitWindow)
            {
                elementsRemoved.Add(m_noteQueue[i]);

                ReturnNoteKnowledgeToPool(m_noteQueue[i]);
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
