// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define APPLICATION_MOONSCRAPER     // Moonscraper doesn't use chart.gameMode because notes might not have charts associated with them, copy-pasting for instance and storing undo-redo

using System;
using System.Collections.Generic;

[System.Serializable]
public class Note : ChartObject 
{
    private readonly ID _classID = ID.Note;

    public override int classID { get { return (int)_classID; } }

    public uint length;
    public int rawNote;
    public GuitarFret guitarFret
    {
        get
        {
            return (GuitarFret)rawNote;
        }
        set
        {
            rawNote = (int)value;
        }
    }

    public DrumPad drumPad
    {
        get
        {
            return (DrumPad)guitarFret;
        }
    }

    public GHLiveGuitarFret ghliveGuitarFret
    {
        get
        {
            return (GHLiveGuitarFret)rawNote;
        }
        set
        {
            rawNote = (int)value;
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
                int _rawNote,
                uint _sustain = 0,
                Flags _flags = Flags.NONE) : base(_position)
    {
        length = _sustain;
        flags = _flags;
        rawNote = _rawNote;

        previous = null;
        next = null;
    }

    public Note(uint _position, 
                GuitarFret _fret_type, 
                uint _sustain = 0, 
                Flags _flags = Flags.NONE) : base(_position)
    {
        length = _sustain;
        flags = _flags;
        guitarFret = _fret_type;

        previous = null;
        next = null;
    }

    public Note (Note note) : base(note.position)
    {
        position = note.position;
        length = note.length;
        flags = note.flags;
        rawNote = note.rawNote;
    }

    public enum GuitarFret
    {
        // Assign to the sprite array position
        GREEN = 0, RED = 1, YELLOW = 2, BLUE = 3, ORANGE = 4, OPEN = 5
    }

    public enum DrumPad
    {
        // Wrapper to account for how the frets change colours between the drums and guitar tracks from the GH series
        KICK = GuitarFret.OPEN, RED = GuitarFret.GREEN, YELLOW = GuitarFret.RED, BLUE = GuitarFret.YELLOW, ORANGE = GuitarFret.BLUE, GREEN = GuitarFret.ORANGE
    }

    public enum GHLiveGuitarFret
    {
        // Assign to the sprite array position
        //WHITE_1, BLACK_1, WHITE_2, BLACK_2, WHITE_3, BLACK_3, OPEN
        BLACK_1,  BLACK_2, BLACK_3, WHITE_1, WHITE_2, WHITE_3, OPEN
    }

    public enum NoteType
    {
        Natural, Strum, Hopo, Tap
    }

    public enum SpecialType
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

    private Chart.GameMode gameMode
    {
        get
        {
            if (chart != null)
                return chart.gameMode;
            else
            {
#if APPLICATION_MOONSCRAPER
                return ChartEditor.GetInstance().currentChart.gameMode;
#else
                return Chart.GameMode.Unrecognised;
#endif
            }
        }
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

    // Deprecated
    internal override string GetSaveString()
    {
        int fretNumber = (int)guitarFret;

        if (guitarFret == GuitarFret.OPEN)
            fretNumber = 7;

        return Globals.TABSPACE + position + " = N " + fretNumber + " " + length + Globals.LINE_ENDING;          // 48 = N 2 0
    }

    public override SongObject Clone()
    {
        return new Note(this);
    }

    public override bool AllValuesCompare<T>(T songObject)
    {
        if (this == songObject && (songObject as Note).length == length && (songObject as Note).rawNote == rawNote && (songObject as Note).flags == flags)
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
                if (prevIsChord || (!prevIsChord && rawNote != previous.rawNote))
                {
                    // Check distance from previous note 
                    int HOPODistance = (int)(65 * song.resolution / Song.STANDARD_BEAT_RESOLUTION);

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
                mask |= (1 << note.rawNote);

            return mask;
        }
    }

    /// <summary>
    /// Live calculation of what Note_Type this note would currently be. 
    /// </summary>
    public NoteType type
    {
        get
        {
            if (!IsOpenNote() && (flags & Flags.TAP) == Flags.TAP)
            {
                return NoteType.Tap;
            }
            else
            {
                if (IsHopo)
                    return NoteType.Hopo;
                else
                    return NoteType.Strum;
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

    public ActionHistory.Modify CapSustain(Note cap)
    {
        if (cap == null)
            return null;

        Note originalNote = (Note)this.Clone();

        // Cap sustain length
        if (cap.position <= position)
            length = 0;
        else if (position + length > cap.position)        // Sustain extends beyond cap note 
        {
            length = cap.position - position;
        }

        uint gapDis = (uint)(song.resolution * 4.0f / GameSettings.sustainGap);

        if (GameSettings.sustainGapEnabled && length > 0 && (position + length > cap.position - gapDis))
        {
            if ((int)(cap.position - gapDis - position) > 0)
                length = cap.position - gapDis - position;
            else
                length = 0;
        }

        if (originalNote.length != length)
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
            if (!GameSettings.extendedSustainsEnabled)
            {
                if ((next.IsOpenNote() || (position < next.position)) && position != next.position)
                    return next;
            }
            else
            {
                if ((!IsOpenNote() && next.IsOpenNote() && !(gameMode == Chart.GameMode.Drums)) || (next.rawNote == rawNote))
                    return next;
            }

            next = next.next;
        }

        return null;
    }

    public bool IsOpenNote()
    {
        if (gameMode == Chart.GameMode.GHLGuitar)
            return ghliveGuitarFret == GHLiveGuitarFret.OPEN;
        else
            return guitarFret == GuitarFret.OPEN;
    }

    /// <summary>
    /// Calculates and sets the sustain length based the tick position it should end at. Will be a length of 0 if the note position is greater than the specified position.
    /// </summary>
    /// <param name="pos">The end-point for the sustain.</param>
    public void SetSustainByPos(uint pos)
    {
        if (pos > position)
            length = pos - position;
        else
            length = 0;

        // Cap the sustain
        Note nextFret;
            nextFret = FindNextSameFretWithinSustainExtendedCheck();

        if (nextFret != null)
        {
            CapSustain(nextFret);
        }
    }

    public void SetType(NoteType type)
    {
        flags = Flags.NONE;
        switch (type)
        {
            case (NoteType.Strum):
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

            case (NoteType.Hopo):
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

            case (NoteType.Tap):
                if (!IsOpenNote())
                    flags |= Note.Flags.TAP;
                break;

            default:
                break;
        }

        applyFlagsToChord();
    }

    public int ExpensiveGetExtendedSustainMask()
    {
        int mask = 0;

        if (length > 0 && chart != null)
        {
            int index, length;
            Note[] notes = chart.notes;
            SongObjectHelper.GetRange(notes, position, position + this.length - 1, out index, out length);

            for (int i = index; i < index + length; ++i)
            {
                Note note = notes[i];
                mask |= note.mask;
            }
        }

        return mask;
    }

    public static GuitarFret SaveGuitarNoteToDrumNote(GuitarFret fret_type)
    {
        if (fret_type == GuitarFret.OPEN)
            return GuitarFret.GREEN;
        else if (fret_type == GuitarFret.ORANGE)
            return GuitarFret.OPEN;
        else
            return fret_type + 1;
    }

    public static GuitarFret LoadDrumNoteToGuitarNote(GuitarFret fret_type)
    {
        if (fret_type == GuitarFret.OPEN)
            return GuitarFret.ORANGE;
        else if (fret_type == GuitarFret.GREEN)
            return GuitarFret.OPEN;
        else
            return fret_type - 1;
    }
}
