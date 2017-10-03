// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections.Generic;

[System.Serializable]
public class Note : ChartObject 
{
    private readonly ID _classID = ID.Note;

    public override int classID { get { return (int)_classID; } }

    public uint sustain_length;
    public int rawNote;
    public Fret_Type fret_type
    {
        get
        {
            return (Fret_Type)rawNote;
        }
        set
        {
            rawNote = (int)value;
        }
    }

    public Drum_Fret_Type drum_fret_type
    {
        get
        {
            return (Drum_Fret_Type)fret_type;
        }
    }

    /// <summary>
    /// Properties, such as forced or taps, are stored here in a bitwise format.
    /// </summary>
    public Flags flags;

    /// <summary>
    /// The previous note in the linked-list.
    /// </summary>
    public Note previous;
    /// <summary>
    /// The next note in the linked-list.
    /// </summary>
    public Note next;

    new public NoteController controller {
        get { return (NoteController)base.controller; }
        set { base.controller = value; }
    }

    public Note(uint _position, 
                Fret_Type _fret_type, 
                uint _sustain = 0, 
                Flags _flags = Flags.NONE) : base(_position)
    {
        sustain_length = _sustain;
        flags = _flags;
        fret_type = _fret_type;

        previous = null;
        next = null;
    }

    public Note (Note note) : base(note.position)
    {
        position = note.position;
        sustain_length = note.sustain_length;
        flags = note.flags;
        fret_type = note.fret_type;
    }

    public enum Fret_Type
    {
        // Assign to the sprite array position
        GREEN = 0, RED = 1, YELLOW = 2, BLUE = 3, ORANGE = 4, OPEN = 5
    }

    public enum Drum_Fret_Type
    {
        // Wrapper to account for how the frets change colours between the drums and guitar tracks from the GH series
        KICK = Fret_Type.OPEN, RED = Fret_Type.GREEN, YELLOW = Fret_Type.RED, BLUE = Fret_Type.YELLOW, ORANGE = Fret_Type.BLUE, GREEN = Fret_Type.ORANGE
    }


    public enum Note_Type
    {
        Natural, Strum, Hopo, Tap
    }

    public enum Special_Type
    {
        NONE, STAR_POW, BATTLE
    }

    [Flags]
    public enum Flags
    {
        NONE = 0,
        FORCED = 1,
        TAP = 2
    }

    public bool forced
    {
        get
        {
            return (flags & Flags.FORCED) == Flags.FORCED;
        }
        set
        {
            if (value)
                flags = flags | Flags.FORCED;
            else
                flags = flags & ~Flags.FORCED;
        }
    }

    /// <summary>
    /// Gets the next note in the linked-list that's not part of this note's chord.
    /// </summary>
    public Note nextSeperateNote
    {
        get
        {
            Note nextNote = next;
            while (nextNote != null && nextNote.position == position)
                nextNote = nextNote.next;
            return nextNote;
        }
    }

    /// <summary>
    /// Gets the previous note in the linked-list that's not part of this note's chord.
    /// </summary>
    public Note previousSeperateNote
    {
        get
        {
            Note previousNote = previous;
            while (previousNote != null && previousNote.position == position)
                previousNote = previousNote.previous;
            return previousNote;
        }
    }

    internal override string GetSaveString()
    {
        int fretNumber = (int)fret_type;

        if (fret_type == Fret_Type.OPEN)
            fretNumber = 7;

        return Globals.TABSPACE + position + " = N " + fretNumber + " " + sustain_length + Globals.LINE_ENDING;          // 48 = N 2 0
    }

    public override SongObject Clone()
    {
        return new Note(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as Note).sustain_length == sustain_length && (songObject as Note).rawNote == rawNote && (songObject as Note).flags == flags)
            return true;
        else
            return false;
    }

    public string GetFlagsSaveString()
    {
        string saveString = string.Empty;

        if ((flags & Flags.FORCED) == Flags.FORCED)
            saveString += Globals.TABSPACE + position + " = N 5 0 " + Globals.LINE_ENDING;

        if ((flags & Flags.TAP) == Flags.TAP)
            saveString += Globals.TABSPACE + position + " = N 6 0 " + Globals.LINE_ENDING;

        return saveString;
    }
    
    protected override bool Equals(SongObject b)
    {
        if (b.GetType() == typeof(Note))
        {
            Note realB = b as Note;
            if (position == realB.position && rawNote == realB.rawNote)
                return true;
            else
                return false;
        }
        else
            return base.Equals(b);
    }

    protected override bool LessThan(SongObject b)
    {
        if (b.GetType() == typeof(Note))
        {
            Note realB = b as Note;
            if (position < b.position)
                return true;
            else if (position == b.position)
            {
                if (rawNote < realB.rawNote)
                    return true;
            }

            return false;
        }
        else
            return base.LessThan(b);
    }
    
    public static void groupAddFlags (Note[] notes, Flags flag)
    {
        for (int i = 0; i < notes.Length; ++i)
        {
            notes[i].flags = notes[i].flags | flag;
        }
    }

    public bool IsChord
    {
        get
        {
            return ((previous != null && previous.position == position) || (next != null && next.position == position));
        }
    }

    /// <summary>
    /// Ignores the note's forced flag when determining whether it would be a hopo or not
    /// </summary>
    public bool IsNaturalHopo
    {
        get
        {
            bool HOPO = false;

            if (!IsChord && previous != null)
            {
                bool prevIsChord = previous.IsChord;
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (prevIsChord || (!prevIsChord && fret_type != previous.fret_type))
                {
                    // Check distance from previous note 
                    int HOPODistance = (int)(65 * song.resolution / Globals.STANDARD_BEAT_RESOLUTION);

                    if (position - previous.position <= HOPODistance)
                        HOPO = true;
                }
            }

            return HOPO;
        }
    }

    /// <summary>
    /// Would this note be a hopo or not? (Ignores whether the note's tap flag is set or not.)
    /// </summary>
    bool IsHopo
    {
        get
        {
            bool HOPO = IsNaturalHopo;

            // Check if forced
            if (forced)
                HOPO = !HOPO;

            return HOPO;
        }
    }

    /// <summary>
    /// Returns a bit mask representing the whole note's chord. For example, a green, red and blue chord would have a mask of 0000 1011. A yellow and orange chord would have a mask of 0001 0100. 
    /// Shifting occurs accoring the values of the Fret_Type enum, so open notes currently output with a mask of 0010 0000.
    /// </summary>
    public int mask
    {
        get
        {
            Note[] chord = GetChord();
            int mask = 0;

            foreach (Note note in chord)
                mask |= (1 << (int)note.fret_type);

            return mask;
        }
    }

    /// <summary>
    /// Live calculation of what Note_Type this note would currently be. 
    /// </summary>
    public Note_Type type
    {
        get
        {
            if (fret_type != Fret_Type.OPEN && (flags & Flags.TAP) == Flags.TAP)
            {
                return Note_Type.Tap;
            }
            else
            {
                if (IsHopo)
                    return Note_Type.Hopo;
                else
                    return Note_Type.Strum;
            }
        }
    }

    /// <summary>
    /// Gets all the notes (including this one) that share the same tick position as this one.
    /// </summary>
    /// <returns>Returns an array of all the notes currently sharing the same tick position as this note.</returns>
    public Note[] GetChord()
    {
        List<Note> chord = new List<Note>();
        chord.Add(this);

        Note previous = this.previous;
        while (previous != null && previous.position == this.position)
        {
            chord.Add(previous);
            previous = previous.previous;
        }

        Note next = this.next;
        while (next != null && next.position == this.position)
        {
            chord.Add(next);
            next = next.next;
        }

        return chord.ToArray();
    }

    public void applyFlagsToChord()
    {
        Note[] chordNotes = GetChord();

        foreach (Note chordNote in chordNotes)
        {
            chordNote.flags = flags;
        }
    }

    public bool CannotBeForcedCheck
    {
        get
        {
            Note seperatePrevious = previousSeperateNote;

            if ((seperatePrevious == null) || (seperatePrevious != null && mask == seperatePrevious.mask))
                return true;

            return false;
        }
    }

    public static Note[] GetPreviousOfSustains(Note startNote)
    {
        List<Note> list = new List<Note>(6);

        Note previous = startNote.previous;

        const int allVisited = 31; // 0001 1111
        int noteTypeVisited = 0;

        while (previous != null && noteTypeVisited < allVisited)
        {
            if (previous.fret_type == Note.Fret_Type.OPEN)
            {
                if (Globals.extendedSustainsEnabled)
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
                switch (previous.fret_type)
                {
                    case (Note.Fret_Type.GREEN):
                        if ((noteTypeVisited & (1 << (int)Note.Fret_Type.GREEN)) == 0)
                        {
                            list.Add(previous);
                            noteTypeVisited |= 1 << (int)Note.Fret_Type.GREEN;
                        }
                        break;
                    case (Note.Fret_Type.RED):
                        if ((noteTypeVisited & (1 << (int)Note.Fret_Type.RED)) == 0)
                        {
                            list.Add(previous);
                            noteTypeVisited |= 1 << (int)Note.Fret_Type.RED;
                        }
                        break;
                    case (Note.Fret_Type.YELLOW):
                        if ((noteTypeVisited & (1 << (int)Note.Fret_Type.YELLOW)) == 0)
                        {
                            list.Add(previous);
                            noteTypeVisited |= 1 << (int)Note.Fret_Type.YELLOW;
                        }
                        break;
                    case (Note.Fret_Type.BLUE):
                        if ((noteTypeVisited & (1 << (int)Note.Fret_Type.BLUE)) == 0)
                        {
                            list.Add(previous);
                            noteTypeVisited |= 1 << (int)Note.Fret_Type.BLUE;
                        }
                        break;
                    case (Note.Fret_Type.ORANGE):
                        if ((noteTypeVisited & (1 << (int)Note.Fret_Type.ORANGE)) == 0)
                        {
                            list.Add(previous);
                            noteTypeVisited |= 1 << (int)Note.Fret_Type.ORANGE;
                        }
                        break;
                    default:
                        break;
                }
            }

            previous = previous.previous;
        }

        return list.ToArray();
    }

    public ActionHistory.Modify CapSustain(Note cap)
    {
        if (cap == null)
            return null;

        Note originalNote = (Note)this.Clone();

        // Cap sustain length
        if (cap.position <= position)
            sustain_length = 0;
        else if (position + sustain_length > cap.position)        // Sustain extends beyond cap note 
        {
            sustain_length = cap.position - position;
        }

        uint gapDis = (uint)(song.resolution * 4.0f / Globals.sustainGap);

        if (Globals.sustainGapEnabled && sustain_length > 0 && (position + sustain_length > cap.position - gapDis))
        {
            if ((int)(cap.position - gapDis - position) > 0)
                sustain_length = cap.position - gapDis - position;
            else
                sustain_length = 0;
        }

        if (originalNote.sustain_length != sustain_length)
            return new ActionHistory.Modify(originalNote, this);
        else
            return null;
    }

    public override void Delete(bool update = true)
    {
        base.Delete(update);

        // Update the previous note in the case of chords with 2 notes
        if (previous != null && previous.controller)
            previous.controller.UpdateSongObject();
        if (next != null && next.controller)
            next.controller.UpdateSongObject();
    }

    public Note FindNextSameFretWithinSustainExtendedCheck()
    {
        Note next = this.next;

        while (next != null)
        {
            if (!Globals.extendedSustainsEnabled)
            {
                if ((next.fret_type == Note.Fret_Type.OPEN || (position < next.position)) && position != next.position)
                    return next;
                //else if (next.position >= note.position + note.sustain_length)      // Stop searching early
                //return null;
            }
            else
            {
                if ((fret_type != Fret_Type.OPEN && next.fret_type == Note.Fret_Type.OPEN && !Globals.drumMode) || (next.fret_type == fret_type))
                    return next;
                //else if (next.position >= note.position + note.sustain_length)      // Stop searching early
                //return null;
            }

            next = next.next;
        }

        return null;
    }

    /// <summary>
    /// Calculates and sets the sustain length based the tick position it should end at. Will be a length of 0 if the note position is greater than the specified position.
    /// </summary>
    /// <param name="pos">The end-point for the sustain.</param>
    public void SetSustainByPos(uint pos)
    {
        if (pos > position)
            sustain_length = pos - position;
        else
            sustain_length = 0;

        // Cap the sustain
        Note nextFret;
        /*
        if (fret_type == Fret_Type.OPEN)
            nextFret = next;
        else*/
            nextFret = FindNextSameFretWithinSustainExtendedCheck();

        if (nextFret != null)
        {
            CapSustain(nextFret);
        }
    }

    public void SetType(Note_Type type)
    {
        flags = Flags.NONE;
        switch (type)
        {
            case (Note_Type.Strum):
                if (IsChord)
                    flags &= ~Note.Flags.FORCED;
                else
                {
                    if (IsNaturalHopo)
                        flags |= Note.Flags.FORCED;
                    else
                        flags &= ~Note.Flags.FORCED;
                }

                break;

            case (Note_Type.Hopo):
                if (!CannotBeForcedCheck)
                {
                    if (IsChord)
                        flags |= Note.Flags.FORCED;
                    else
                    {
                        if (!IsNaturalHopo)
                            flags |= Note.Flags.FORCED;
                        else
                            flags &= ~Note.Flags.FORCED;
                    }
                }
                break;

            case (Note_Type.Tap):
                if (fret_type != Fret_Type.OPEN)
                    flags |= Note.Flags.TAP;
                break;

            default:
                break;
        }

        applyFlagsToChord();
    }

    public static Fret_Type SaveGuitarNoteToDrumNote(Fret_Type fret_type)
    {
        if (fret_type == Fret_Type.OPEN)
            return Fret_Type.GREEN;
        else if (fret_type == Fret_Type.ORANGE)
            return Fret_Type.OPEN;
        else
            return fret_type + 1;
    }

    public static Fret_Type LoadDrumNoteToGuitarNote(Fret_Type fret_type)
    {
        if (fret_type == Fret_Type.OPEN)
            return Fret_Type.ORANGE;
        else if (fret_type == Fret_Type.GREEN)
            return Fret_Type.OPEN;
        else
            return fret_type - 1;
    }
}
