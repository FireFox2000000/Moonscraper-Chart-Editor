// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

#define APPLICATION_MOONSCRAPER     // Moonscraper doesn't use chart.gameMode because notes might not have charts associated with them, copy-pasting for instance and storing undo-redo

using System;
using System.Collections.Generic;

[System.Serializable]
public class Note : ChartObject 
{
    public enum DrumPad
    {
        // Wrapper to account for how the frets change colours between the drums and guitar tracks from the GH series
        Kick = GuitarFret.Open,
        Red = GuitarFret.Green,
        Yellow = GuitarFret.Red,
        Blue = GuitarFret.Yellow,
        Orange = GuitarFret.Blue,
        Green = GuitarFret.Orange
    }

    public enum GHLiveGuitarFret
    {
        // Assign to the sprite array position
        //WHITE_1, BLACK_1, WHITE_2, BLACK_2, WHITE_3, BLACK_3, OPEN
        Black1,
        Black2,
        Black3,
        White1,
        White2,
        White3,
        Open
    }

    public enum NoteType
    {
        Natural,
        Strum,
        Hopo,
        Tap
    }

    public enum SpecialType
    {
        None,
        StarPower,
        Battle
    }

    [Flags]
    public enum Flags
    {
        None = 0,
        Forced = 1,
        Tap = 2
    }

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
                Flags _flags = Flags.None) : base(_position)
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
                Flags _flags = Flags.None) : base(_position)
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
        Green = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3,
        Orange = 4,
        Open = 5
    }

    public Chart.GameMode gameMode
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
            return (flags & Flags.Forced) == Flags.Forced;
        }
        set
        {
            if (value)
                flags = flags | Flags.Forced;
            else
                flags = flags & ~Flags.Forced;
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
            Note[] chord = this.GetChord();
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
            if (!this.IsOpenNote() && (flags & Flags.Tap) == Flags.Tap)
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

    public bool cannotBeForced
    {
        get
        {
            Note seperatePrevious = previousSeperateNote;

            if ((seperatePrevious == null) || (seperatePrevious != null && mask == seperatePrevious.mask))
                return true;

            return false;
        }
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

    // Deprecated
    internal override string GetSaveString()
    {
        int fretNumber = (int)guitarFret;

        if (guitarFret == GuitarFret.Open)
            fretNumber = 7;

        return Globals.TABSPACE + position + " = N " + fretNumber + " " + length + Globals.LINE_ENDING;          // 48 = N 2 0
    }

    public string GetFlagsSaveString()
    {
        string saveString = string.Empty;

        if ((flags & Flags.Forced) == Flags.Forced)
            saveString += Globals.TABSPACE + position + " = N 5 0 " + Globals.LINE_ENDING;

        if ((flags & Flags.Tap) == Flags.Tap)
            saveString += Globals.TABSPACE + position + " = N 6 0 " + Globals.LINE_ENDING;

        return saveString;
    }
}
