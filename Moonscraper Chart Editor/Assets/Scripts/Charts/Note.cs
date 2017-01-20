using UnityEngine;
using System;
using System.Collections.Generic;

public class Note : ChartObject 
{
    private readonly ID _classID = ID.Note;

    public override int classID { get { return (int)_classID; } }

    public uint sustain_length;
    public Fret_Type fret_type;

    public Flags flags;

    public Note previous;
    public Note next;

    NoteController _controller = null;
    new public NoteController controller {
        get { return _controller; }
        set { _controller = value; base.controller = value; }
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

    public enum Note_Type
    {
        STRUM, HOPO, TAP
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

    public override string GetSaveString()
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
        if (this == songObject && (songObject as Note).sustain_length == sustain_length && (songObject as Note).fret_type == fret_type && (songObject as Note).flags == flags)
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
            if (position == realB.position && fret_type == realB.fret_type)
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
                if (fret_type < realB.fret_type)
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
            if (previous != null && previous.position == position)
                return true;
            else if (next != null && next.position == position)
                return true;
            else
                return false;
        }
    }

    public bool IsNaturalHopo
    {
        get
        {
            bool HOPO = false;

            if (!IsChord && previous != null)
            {
                // Need to consider whether the previous note was a chord, and if they are the same type of note
                if (previous.IsChord || (!previous.IsChord && fret_type != previous.fret_type))
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

    public Note_Type type
    {
        get
        {
            if (fret_type != Fret_Type.OPEN && (flags & Flags.TAP) == Flags.TAP)
            {
                return Note_Type.TAP;
            }
            else
            {
                if (IsHopo)
                    return Note_Type.HOPO;
                else
                    return Note_Type.STRUM;
            }
        }
    }

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
            Note seperatePrevious = previous;
            while (seperatePrevious != null && seperatePrevious.position == position)
                seperatePrevious = seperatePrevious.previous;

            /*if ((previous == null) || (previous != null && !IsChord && !previous.IsChord && previous.fret_type == fret_type))*/
            if ((seperatePrevious == null) || (seperatePrevious != null && mask == seperatePrevious.mask))
                return true;
            /*
            else
            {
                Note[] chordNotes = GetChord();

                foreach (Note chordNote in chordNotes)
                {
                    if (chordNote.previous == null)
                        return true;
                }
                return false;
            }*/

            return false;
        }
    }

    public static Note[] GetPreviousOfSustains(Note startNote)
    {
        List<Note> list = new List<Note>();

        Note previous = startNote.previous;

        const int allVisited = 31; // 0001 1111
        int noteTypeVisited = 0;

        while (previous != null && noteTypeVisited < allVisited)
        {
            if (previous.fret_type == Note.Fret_Type.OPEN)
            {
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
}
