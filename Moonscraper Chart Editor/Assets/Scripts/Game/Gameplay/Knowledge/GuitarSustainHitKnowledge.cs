// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using MoonscraperChartEditor.Song;

public class GuitarSustainHitKnowledge {

    public struct SustainKnowledge
    {
        public Note note;
        public int extendedSustainMask;
        public bool isExtendedSustain;
    }

    List<SustainKnowledge> m_currentSustains = new List<SustainKnowledge>();
    int m_extendedSustainsMask = 0;

    public List<SustainKnowledge> currentSustains
    {
        get
        {
            return m_currentSustains;
        }
    }

    public int extendedSustainsMask
    {
        get
        {
            return m_extendedSustainsMask;
        }
    }

    // Update is called once per frame
    public void Update (float time) {

        m_extendedSustainsMask = currentSustains.Count > 0 && currentSustains[0].isExtendedSustain ? currentSustains[0].extendedSustainMask : 0;

        foreach (SustainKnowledge sustain in currentSustains.ToArray())     // Take a copy so we can remove as we go
        {
            if (!sustain.note.controller || sustain.note.controller.sustainBroken)
                currentSustains.Remove(sustain);

            Note note = sustain.note;
            Song song = note.song;

            if (song.TickToTime(note.tick + note.length, song.resolution) < time)
                currentSustains.Remove(sustain);
        }
    }

    public void Reset()
    {
        m_currentSustains.Clear();
    }

    public void Add(Note note)
    {
        if (note.length > 0)
        {
            SustainKnowledge newSustain;
            newSustain.note = note;
            newSustain.extendedSustainMask = note.ExpensiveGetExtendedSustainMask();
            newSustain.isExtendedSustain = newSustain.extendedSustainMask != 0 && newSustain.extendedSustainMask != note.mask;

            m_currentSustains.Add(newSustain);
        }
    }
}
