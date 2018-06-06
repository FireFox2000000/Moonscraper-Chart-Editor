using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoteFunctions {

    public static void groupAddFlags(Note[] notes, Note.Flags flag)
    {
        for (int i = 0; i < notes.Length; ++i)
        {
            notes[i].flags = notes[i].flags | flag;
        }
    }

    /// <summary>
    /// Gets all the notes (including this one) that share the same tick position as this one.
    /// </summary>
    /// <returns>Returns an array of all the notes currently sharing the same tick position as this note.</returns>
    public static Note[] GetChord(this Note note)
    {
        List<Note> chord = new List<Note>();
        chord.Add(note);

        Note previous = note.previous;
        while (previous != null && previous.position == note.position)
        {
            chord.Add(previous);
            previous = previous.previous;
        }

        Note next = note.next;
        while (next != null && next.position == note.position)
        {
            chord.Add(next);
            next = next.next;
        }

        return chord.ToArray();
    }

    public static void ApplyFlagsToChord(this Note note)
    {
        Note[] chordNotes = note.GetChord();

        foreach (Note chordNote in chordNotes)
        {
            chordNote.flags = note.flags;
        }
    }

    public static Note[] GetPreviousOfSustains(Note startNote)
    {
        List<Note> list = new List<Note>(6);

        Note previous = startNote.previous;

        int allVisited = startNote.gameMode == Chart.GameMode.GHLGuitar ? 63 : 31; // 0011 1111 for ghlive, 0001 1111 for standard
        int noteTypeVisited = 0;

        while (previous != null && noteTypeVisited < allVisited)
        {
            if (previous.IsOpenNote())
            {
                if (GameSettings.extendedSustainsEnabled)
                {
                    list.Add(previous);
                    return list.ToArray();
                }

                else if (list.Count > 0)
                    return list.ToArray();
                else
                    return new Note[] { previous };
            }
            else if (previous.position < startNote.position)
            {
                if ((noteTypeVisited & (1 << previous.rawNote)) == 0)
                {
                    list.Add(previous);
                    noteTypeVisited |= 1 << previous.rawNote;
                }
            }

            previous = previous.previous;
        }

        return list.ToArray();
    }

    public static ActionHistory.Modify CapSustain(this Note note, Note cap)
    {
        if (cap == null)
            return null;

        Note originalNote = (Note)note.Clone();

        // Cap sustain length
        if (cap.position <= note.position)
            note.length = 0;
        else if (note.position + note.length > cap.position)        // Sustain extends beyond cap note 
        {
            note.length = cap.position - note.position;
        }

        uint gapDis = (uint)(note.song.resolution * 4.0f / GameSettings.sustainGap);

        if (GameSettings.sustainGapEnabled && note.length > 0 && (note.position + note.length > cap.position - gapDis))
        {
            if ((int)(cap.position - gapDis - note.position) > 0)
                note.length = cap.position - gapDis - note.position;
            else
                note.length = 0;
        }

        if (originalNote.length != note.length)
            return new ActionHistory.Modify(originalNote, note);
        else
            return null;
    }

    public static Note FindNextSameFretWithinSustainExtendedCheck(this Note note)
    {
        Note next = note.next;

        while (next != null)
        {
            if (!GameSettings.extendedSustainsEnabled)
            {
                if ((next.IsOpenNote() || (note.position < next.position)) && note.position != next.position)
                    return next;
            }
            else
            {
                if ((!note.IsOpenNote() && next.IsOpenNote() && !(note.gameMode == Chart.GameMode.Drums)) || (next.rawNote == note.rawNote))
                    return next;
            }

            next = next.next;
        }

        return null;
    }

    public static bool IsOpenNote(this Note note)
    {
        if (note.gameMode == Chart.GameMode.GHLGuitar)
            return note.ghliveGuitarFret == Note.GHLiveGuitarFret.Open;
        else
            return note.guitarFret == Note.GuitarFret.Open;
    }

    /// <summary>
    /// Calculates and sets the sustain length based the tick position it should end at. Will be a length of 0 if the note position is greater than the specified position.
    /// </summary>
    /// <param name="pos">The end-point for the sustain.</param>
    public static void SetSustainByPos(this Note note, uint pos)
    {
        if (pos > note.position)
            note.length = pos - note.position;
        else
            note.length = 0;

        // Cap the sustain
        Note nextFret;
        nextFret = note.FindNextSameFretWithinSustainExtendedCheck();

        if (nextFret != null)
        {
            note.CapSustain(nextFret);
        }
    }

    public static void SetType(this Note note, Note.NoteType type)
    {
        note.flags = Note.Flags.None;
        switch (type)
        {
            case (Note.NoteType.Strum):
                if (note.IsChord)
                    note.flags &= ~Note.Flags.Forced;
                else
                {
                    if (note.IsNaturalHopo)
                        note.flags |= Note.Flags.Forced;
                    else
                        note.flags &= ~Note.Flags.Forced;
                }

                break;

            case (Note.NoteType.Hopo):
                if (!note.cannotBeForced)
                {
                    if (note.IsChord)
                        note.flags |= Note.Flags.Forced;
                    else
                    {
                        if (!note.IsNaturalHopo)
                            note.flags |= Note.Flags.Forced;
                        else
                            note.flags &= ~Note.Flags.Forced;
                    }
                }
                break;

            case (Note.NoteType.Tap):
                if (!note.IsOpenNote())
                    note.flags |= Note.Flags.Tap;
                break;

            default:
                break;
        }

        note.ApplyFlagsToChord();
    }

    public static int ExpensiveGetExtendedSustainMask(this Note note)
    {
        int mask = 0;

        if (note.length > 0 && note.chart != null)
        {
            int index, length;
            Note[] notes = note.chart.notes;
            SongObjectHelper.GetRange(notes, note.position, note.position + note.length - 1, out index, out length);

            for (int i = index; i < index + length; ++i)
            {
                mask |= notes[i].mask;
            }
        }

        return mask;
    }

    public static Note.GuitarFret SaveGuitarNoteToDrumNote(Note.GuitarFret fret_type)
    {
        if (fret_type == Note.GuitarFret.Open)
            return Note.GuitarFret.Green;
        else if (fret_type == Note.GuitarFret.Orange)
            return Note.GuitarFret.Open;
        else
            return fret_type + 1;
    }

    public static Note.GuitarFret LoadDrumNoteToGuitarNote(Note.GuitarFret fret_type)
    {
        if (fret_type == Note.GuitarFret.Open)
            return Note.GuitarFret.Orange;
        else if (fret_type == Note.GuitarFret.Green)
            return Note.GuitarFret.Open;
        else
            return fret_type - 1;
    }
}
